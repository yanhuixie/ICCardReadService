namespace ICCardService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.ICCardServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ICCardServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ICCardServiceProcessInstaller
            // 
            this.ICCardServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ICCardServiceProcessInstaller.Password = null;
            this.ICCardServiceProcessInstaller.Username = null;
            // 
            // ICCardServiceInstaller
            // 
            this.ICCardServiceInstaller.Description = "从IC卡读卡器读取卡内信息";
            this.ICCardServiceInstaller.DisplayName = "IC卡读卡服务";
            this.ICCardServiceInstaller.ServiceName = "ICCardService";
            this.ICCardServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ICCardServiceProcessInstaller,
            this.ICCardServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ICCardServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ICCardServiceInstaller;
    }
}