using Microsoft.Extensions.DependencyInjection;
using Reactive.Bindings;
using System.Windows.Controls;
using System;

namespace Extraordinary.App.ViewModels
{

    public class AppViewModel
    {

        private readonly IServiceScopeFactory _ssf;

        public AppViewModel(IServiceScopeFactory ssf)
        {
            this._ssf = ssf;
            this.AppTitle = new ReactiveProperty<string>("Extraordinary");
            this.CurrentPage = new ReactiveProperty<Page>();
        }
        /// <summary>
        /// 应用标题—— 和 AppName不同，这个可以动态变更
        /// </summary>
        public ReactiveProperty<string> AppTitle { get; set; }
        #region 路由
        /// <summary>
        /// 当前页面
        /// </summary>
        public ReactiveProperty<Page> CurrentPage { get; set; }

        public Func<string, Page> MapSourceToPage { get; set; }

        public void NavigateTo(string source)
        {
            if (this.MapSourceToPage == null)
            {
                throw new Exception($"{nameof(MapSourceToPage)}不可为NULL！你是否忘记设置该属性了？");
            }
            var page = MapSourceToPage(source);
            this.CurrentPage.Value = page;
        }
        #endregion
    }
}
