﻿using MASA.Blazor.Experimental.Components;
using MASA.PM.Caller.Callers;
using MASA.PM.Contracts.Base.Model;
using MASA.PM.Contracts.Base.ViewModel;

namespace MASA.PM.UI.Admin.Pages
{
    public partial class Landscape
    {
        private StringNumber _selectedEnvId = 0;
        private StringNumber _selectEnvClusterId = 0;
        private List<EnvironmentsViewModel> _environments = new();
        private List<ClustersViewModel> _clusters = new();
        private List<ProjectViewModel> _projects = new();
        private List<AppViewModel> _apps = new();
        private List<AppViewModel> _projectApps = new();
        private DataModal<UpdateEnvironmentModel> _envFormModel = new();
        private List<ClustersViewModel> _allClusters = new();
        private List<EnvironmentsViewModel> _allEnvs = new();
        private EnvironmentViewModel _envDetail = new();
        private DataModal<UpdateClusterModel> _clusterFormModel = new();
        private ClusterViewModel _clusterDetail = new();

        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        [Inject]
        public EnvironmentCaller EnvironmentCaller { get; set; } = default!;

        [Inject]
        public ClusterCaller ClusterCaller { get; set; } = default!;

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _environments = await EnvironmentCaller.GetListAsync();
                if (_environments.Any())
                {
                    _selectedEnvId = _environments[0].Id;
                    _clusters = await GetClustersByEnvIdAsync(_environments[0].Id);
                }

                StateHasChanged();
            }
        }

        private async Task<List<ClustersViewModel>> GetClustersByEnvIdAsync(int envId, bool isFetchProjects = true)
        {
            _selectedEnvId = envId;
            _clusters = await ClusterCaller.GetListByEnvIdAsync(envId);
            if (_clusters.Any() && isFetchProjects)
            {
                _selectEnvClusterId = _clusters[0].EnvironmentClusterId;
                _projects = await GetProjectByEnvClusterIdAsync(_clusters[0].EnvironmentClusterId);
            }

            return _clusters;
        }

        private async Task<List<ProjectViewModel>> GetProjectByEnvClusterIdAsync(int envClusterId)
        {
            _selectEnvClusterId = envClusterId;
            _projects = await ProjectCaller.GetListByEnvIdAsync(envClusterId);
            if (_projects.Any())
            {
                _apps = await GetAppByProjectIdAsync(_projects.Select(project => project.Id));
                _projectApps = _apps.Where(app => _projects.Select(p => p.Id).Contains(app.ProjectId)).ToList();
            }

            return _projects;
        }

        private async Task<List<AppViewModel>> GetAppByProjectIdAsync(IEnumerable<int> projectIds)
        {
            var result = await AppCaller.GetListByProjectIdAsync(projectIds.ToList());

            return result;
        }

        private async Task<EnvironmentViewModel> GetEnvAsync(int envId)
        {
            _envDetail = await EnvironmentCaller.GetAsync(envId);
            return _envDetail;
        }

        private async Task EditEnvAsync(int envId)
        {
            var env = await GetEnvAsync(envId);
            await ShowEnvModalAsync(new UpdateEnvironmentModel
            {
                ClusterIds = env.ClusterIds,
                Name = env.Name,
                Description = env.Description,
                EnvironmentId = env.Id
            });
        }

        private async Task ShowEnvModalAsync(UpdateEnvironmentModel? model = null)
        {
            if (model == null)
            {
                _envFormModel.Show();
            }
            else
            {
                _envFormModel.Show(model);
            }

            _allClusters = await ClusterCaller.GetListAsync();
        }

        private async Task SubmitEnv()
        {
            if (!_envFormModel.HasValue)
            {
                var newEnv = await EnvironmentCaller.AddAsync(_envFormModel.Data);
                _environments.Add(newEnv);
            }
            else
            {
                await EnvironmentCaller.UpdateAsync(_envFormModel.Data);
                var env = _environments.First(env => env.Id == _envFormModel.Data.EnvironmentId);
                env.Name = _envFormModel.Data.Name;
            }

            _envFormModel.Hide();
        }

        private async Task<ClusterViewModel> GetClusterAsync(int clusterId)
        {
            _clusterDetail = await ClusterCaller.GetAsync(clusterId);

            return _clusterDetail;
        }

        private async Task EditClusterAsync(int clusterId)
        {
            var cluster = await GetClusterAsync(clusterId);
            await ShowClusterModalAsync(new UpdateClusterModel
            {
                ClusterId = cluster.Id,
                Name = cluster.Name,
                Description = cluster.Description,
                EnvironmentIds = cluster.EnvironmentIds
            });
            
        }

        private async Task ShowClusterModalAsync(UpdateClusterModel? model = null)
        {
            if (model == null)
            {
                _clusterFormModel.Show();
            }
            else
            {
                _clusterFormModel.Show(model);
            }

            _allEnvs = await EnvironmentCaller.GetListAsync();
        }

        private async Task SubmitCluster()
        {
            int newClusterId;
            if (!_clusterFormModel.HasValue)
            {
                var cluster = await ClusterCaller.AddAsync(_clusterFormModel.Data);
                newClusterId = cluster.Id;
            }
            else
            {
                await ClusterCaller.UpdateAsync(_clusterFormModel.Data);
                newClusterId = _clusterFormModel.Data.ClusterId;
            }

            await GetClustersByEnvIdAsync(newClusterId);
            _clusterFormModel.Hide();
        }
    }
}
