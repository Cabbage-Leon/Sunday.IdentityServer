using System;

namespace Sunday.IdentityServer.Models
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    public class SysUserInfo
    {
        public SysUserInfo()
        {
        }

        public SysUserInfo(string loginName, string loginPWD)
        {
            ULoginName = loginName;
            ULoginPWD = loginPWD;
            URealName = ULoginName;
            UStatus = 0;
            UCreateTime = DateTime.Now;
            UUpdateTime = DateTime.Now;
            ULastErrTime = DateTime.Now;
            UErrorCount = 0;
            Name = "";
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string ULoginName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string ULoginPWD { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string URealName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int UStatus { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string URemark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime UCreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public System.DateTime UUpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        ///最后登录时间
        /// </summary>
        public DateTime ULastErrTime { get; set; } = DateTime.Now;

        /// <summary>
        ///错误次数
        /// </summary>
        public int UErrorCount { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string Name { get; set; }

        // 性别
        public int Sex { get; set; } = 0;

        // 年龄
        public int Age { get; set; }

        // 生日
        public DateTime Birth { get; set; } = DateTime.Now;

        // 地址
        public string Addr { get; set; }

        public bool TdIsDelete { get; set; }
    }
}