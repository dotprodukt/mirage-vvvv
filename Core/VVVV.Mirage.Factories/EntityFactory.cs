using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;

using VVVV.Hosting.IO;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Factories;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.DX11.Lib.Effects;

using VVVV.Mirage.Lib.Scene;

namespace VVVV.Mirage.Factories
{
    [ComVisible(false)]
    public class AbstractEntityFactory<T> : AbstractFileFactory<IInternalPluginHost> where T : IDX11ShaderNodeWrapper
    {
        #region Imports
        [Import]
        protected DotNetPluginFactory FDotNetFactory;
        [Import]
        protected IORegistry FIORegistry;
        [Import]
        protected ISolution FSolution;
        [Import]
        protected ILogger Logger { get; set; }
        #endregion

        private CompositionContainer FParentContainer;

        private readonly Dictionary<string, FXProject> FProjects;
        private readonly Dictionary<FXProject, INodeInfo> FProjectNodeInfo;
        private readonly Dictionary<IPluginBase, PluginContainer> FPluginContainers;

        protected abstract string NodeCategory { get; }
        protected abstract string NodeVersion { get; }

        public event PluginCreatedDelegate PluginCreated;
        public event PluginDeletedDelegate PluginDeleted;


        public AbstractEntityFactory(CompositionContainer parentContainer, string fileExtension) : base(fileExtension)
        {
            FParentContainer = parentContainer;

            FProjects = new Dictionary<string, FXProject>();
            FProjectNodeInfo = new Dictionary<FXProject, INodeInfo>();
            FPluginContainers = new Dictionary<IPluginBase, PluginContainer>();
        }

        #region AbstractFileFactory members
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            var project = CreateProject(filename);
            yield return LoadNodeInfoFromEffect(filename, project);
        }

        protected override void DoAddFile(string filename)
        {
            CreateProject(filename);
            base.DoAddFile(filename);
        }

        protected override void DoRemoveFile(string filename)
        {
            FXProject project;
            if (FProjects.TryGetValue(filename, out project))
            {
                if (FSolution.Projects.CanRemove(project))
                {
                    FSolution.Projects.Remove(project);
                    project.DoCompileEvent -= project_DoCompileEvent;
                }
                FProjects.Remove(filename);
            }

            base.DoRemoveFile(filename);
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            if (nodeInfo.Type != NodeType.Dynamic)
                return false;

            var project = nodeInfo.UserData as FXProject;
            /*if (!project.IsLoaded)
                project.Load();*/

            //compile shader
            //FIncludeHandler.ParentPath = Path.GetDirectoryName(nodeInfo.Filename);
            string code = File.ReadAllText(nodeInfo.Filename);

            //DX11Effect shader;

            //create or update plugin
            if (pluginHost.Plugin == null)
            {
                nodeInfo.AutoEvaluate = false;
                nodeInfo.Arguments = typeof(T).ToString();

                var pluginContainer = new PluginContainer(pluginHost, FIORegistry, FParentContainer, FNodeInfoFactory, FDotNetFactory, typeof(T), nodeInfo);
                pluginHost.Plugin = pluginContainer;

                FPluginContainers[pluginContainer.PluginBase] = pluginContainer;

                var shadernode = pluginContainer.PluginBase as IDX11ShaderNodeWrapper;
                shadernode.Source = nodeInfo;
                shadernode.WantRecompile += new EventHandler(shadernode_WantRecompile);

                //shader = DX11Effect.FromString(code, FIncludeHandler, shadernode.Macros);

                //shadernode.SetShader(shader, true);

                if (this.PluginCreated != null)
                {
                    this.PluginCreated(pluginContainer, pluginHost);
                }
            }
            else
            {
                PluginContainer container = pluginHost.Plugin as PluginContainer;
                var shaderNode = container.PluginBase as IDX11ShaderNodeWrapper;
                //shader = DX11Effect.FromString(code, FIncludeHandler, shaderNode.Macros);
                //shaderNode.SetShader(shader, false);
            }

            //now the effect is compiled in vvvv and we can access the errors
            /*string e = shader.ErrorMessage;//effectHost.GetErrors();
            if (string.IsNullOrEmpty(e))
                e = "";

            this.ParseErrors(e, project, shader);*/



            //and the input pins
            string f = "";// effectHost.GetParameterDescription();
            if (string.IsNullOrEmpty(f))
                f = "";
            project.ParameterDescription = f;

            return true;
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost host)
        {
            var plugin = host.Plugin;

            var disposablePlugin = plugin as IDisposable;
            if (FPluginContainers.ContainsKey(plugin))
            {
                FPluginContainers[plugin].Dispose();
                FPluginContainers.Remove(plugin);
            }

            if (this.PluginDeleted != null)
            {
                this.PluginDeleted(host.Plugin);
            }
            return true;
        }

        protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string filename)
        {
            if (nodeInfo.Type == NodeType.Dynamic)
            {
                var project = nodeInfo.UserData as FXProject;
                /*if (!project.IsLoaded)
                    project.Load();*/

                var projectDir = path;
                var newProjectName = name + this.FileExtension[0];
                var newLocation = new Uri(projectDir.ConcatPath(newProjectName));

                filename = projectDir.ConcatPath(newProjectName);

                project.SaveTo(filename);

                return true;
            }

            return base.CloneNode(nodeInfo, path, name, category, version, out filename);
        }


        #endregion

        private INodeInfo LoadNodeInfoFromEffect(string filename, FXProject project)
        {
            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                Path.GetFileNameWithoutExtension(filename),
                this.NodeCategory, this.NodeVersion,
                filename,
                true);

            nodeInfo.BeginUpdate();
            nodeInfo.Type = NodeType.Dynamic;
            nodeInfo.Factory = this;
            nodeInfo.UserData = project;

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    string author = @"//@author:";
                    string desc = @"//@help:";
                    string tags = @"//@tags:";
                    string credits = @"//@credits:";

                    // Parse lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith(author))
                            nodeInfo.Author = line.Replace(author, "").Trim();

                        else if (line.StartsWith(desc))
                            nodeInfo.Help = line.Replace(desc, "").Trim();

                        else if (line.StartsWith(tags))
                            nodeInfo.Tags = line.Replace(tags, "").Trim();

                        else if (line.StartsWith(credits))
                            nodeInfo.Credits = line.Replace(credits, "").Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, "Effect does not contain detailed info");
                Logger.Log(ex);
            }

            try
            {
                nodeInfo.CommitUpdate();
            }
            catch { }

            return nodeInfo;
        }

        private FXProject CreateProject(string filename)
        {
            FXProject project;
            if (!FProjects.TryGetValue(filename, out project))
            {
                var isValid = false;

                //do some kind of validation here
                using (var sr = new StreamReader(filename))
                {
                    var code = sr.ReadToEnd();
                    code = Regex.Replace(code, @"//.*?\n", "", RegexOptions.Singleline);
                    code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);

                    if (code.Contains("#pragma mirage")) isValid = true;
                }

                if (isValid)
                {
                    project = new FXProject(filename, FHDEHost.ExePath);
                    if (FSolution.Projects.CanAdd(project))
                    {
                        FSolution.Projects.Add(project);
                    }
                    else
                    {
                        // Project was renamed
                        project = FSolution.Projects[project.Name] as FXProject;
                    }

                    project.DoCompileEvent += project_DoCompileEvent;

                    FProjects[filename] = project;
                }
            }

            return project;
        }

        void project_DoCompileEvent(object sender, EventArgs e)
        {
            var project = sender as FXProject;
            var filename = project.LocalPath;

            LoadNodeInfoFromEffect(filename, project);
        }

        void shadernode_WantRecompile(object sender, EventArgs e)
        {
            IDX11ShaderNodeWrapper wrp = (IDX11ShaderNodeWrapper)sender;
            //FIncludeHandler.ParentPath = Path.GetDirectoryName(wrp.Source.Filename);
            string code = File.ReadAllText(wrp.Source.Filename);

            //var shader = DX11Effect.FromString(code, FIncludeHandler, wrp.Macros);
            //wrp.SetShader(shader, false);
        }

    }

    [Export(typeof(IAddonFactory))]
    [Export(typeof(EntityNodeFactory))]
    [ComVisible(false)]
    public class EntityNodeFactory : AbstractEntityFactory<BaseEntityNode>
    {
        [ImportingConstructor()]
        public EntityNodeFactory(CompositionContainer parentContainer) : base(parentContainer,".ent")
        {
        }

        public override string JobStdSubPath
        {
            get { return "mirage/entity"; }
        }

        protected override string NodeCategory
        {
            get { return "Mirage.Entity"; }
        }

        protected override string NodeVersion
        {
            get { return ""; }
        }
    }
}