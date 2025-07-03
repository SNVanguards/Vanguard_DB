namespace VanguardDev_API
{
    using SqlSugar;

    namespace VanguardTest.Models
    {
        /// <summary>
        /// 用户信息表
        /// </summary>
        [SugarTable("Base_User")]
        public class UserEntity
        {
            /// <summary>
            /// 用户主键
            /// </summary>
            [SugarColumn(IsPrimaryKey = true)] // 主键
            public string F_UserId { get; set; }

            /// <summary>
            /// 用户id,与邮件系统对接
            /// </summary>
            public int UserId { get; set; }

            /// <summary>
            /// 是否绑定邮箱系统，0=未绑定，1=已绑定
            /// </summary>
            public int? BindMail { get; set; }

            /// <summary>
            /// 用户编码
            /// </summary>
            public string F_EnCode { get; set; }

            /// <summary>
            /// 登录账户
            /// </summary>
            public string F_Account { get; set; }

            /// <summary>
            /// 登录密码
            /// </summary>
            public string F_Password { get; set; }

            /// <summary>
            /// 密码秘钥
            /// </summary>
            public string F_Secretkey { get; set; }

            /// <summary>
            /// 真实姓名
            /// </summary>
            public string F_RealName { get; set; }

            /// <summary>
            /// 呢称
            /// </summary>
            public string F_NickName { get; set; }

            /// <summary>
            /// 头像
            /// </summary>
            public string F_HeadIcon { get; set; }

            /// <summary>
            /// 快速查询
            /// </summary>
            public string F_QuickQuery { get; set; }

            /// <summary>
            /// 简拼
            /// </summary>
            public string F_SimpleSpelling { get; set; }

            /// <summary>
            /// 性别
            /// </summary>
            public int? F_Gender { get; set; }

            /// <summary>
            /// 生日
            /// </summary>
            public DateTime? F_Birthday { get; set; }

            /// <summary>
            /// 手机
            /// </summary>
            public string F_Mobile { get; set; }

            /// <summary>
            /// 电话
            /// </summary>
            public string F_Telephone { get; set; }

            /// <summary>
            /// 电子邮件
            /// </summary>
            public string F_Email { get; set; }

            /// <summary>
            /// QQ号
            /// </summary>
            public string F_OICQ { get; set; }

            /// <summary>
            /// 微信号
            /// </summary>
            public string F_WeChat { get; set; }

            /// <summary>
            /// MSN
            /// </summary>
            public string F_MSN { get; set; }

            /// <summary>
            /// 主管主键
            /// </summary>
            public string F_ManagerId { get; set; }

            /// <summary>
            /// 主管
            /// </summary>
            public string F_Manager { get; set; }

            /// <summary>
            /// 机构主键
            /// </summary>
            public string F_OrganizeId { get; set; }

            /// <summary>
            /// 部门主键
            /// </summary>
            public string F_DepartmentId { get; set; }

            /// <summary>
            /// 角色主键
            /// </summary>
            public string F_RoleId { get; set; }

            /// <summary>
            /// 岗位主键
            /// </summary>
            public string F_DutyId { get; set; }

            /// <summary>
            /// 岗位名称
            /// </summary>
            public string F_DutyName { get; set; }

            /// <summary>
            /// 职位主键
            /// </summary>
            public string F_PostId { get; set; }

            /// <summary>
            /// 职位名称
            /// </summary>
            public string F_PostName { get; set; }

            /// <summary>
            /// 工作组主键
            /// </summary>
            public string F_WorkGroupId { get; set; }

            /// <summary>
            /// 安全级别
            /// </summary>
            public int? F_SecurityLevel { get; set; }

            /// <summary>
            /// 在线状态
            /// </summary>
            public int? F_UserOnLine { get; set; }

            /// <summary>
            /// 单点登录标识
            /// </summary>
            public int? F_OpenId { get; set; }

            /// <summary>
            /// 密码提示问题
            /// </summary>
            public string F_Question { get; set; }

            /// <summary>
            /// 密码提示答案
            /// </summary>
            public string F_AnswerQuestion { get; set; }

            /// <summary>
            /// 允许多用户同时登录
            /// </summary>
            public int? F_CheckOnLine { get; set; }

            /// <summary>
            /// 允许登录时间开始
            /// </summary>
            public DateTime? F_AllowStartTime { get; set; }

            /// <summary>
            /// 允许登录时间结束
            /// </summary>
            public DateTime? F_AllowEndTime { get; set; }

            /// <summary>
            /// 暂停用户开始日期
            /// </summary>
            public DateTime? F_LockStartDate { get; set; }

            /// <summary>
            /// 暂停用户结束日期
            /// </summary>
            public DateTime? F_LockEndDate { get; set; }

            /// <summary>
            /// 第一次访问时间
            /// </summary>
            public DateTime? F_FirstVisit { get; set; }

            /// <summary>
            /// 上一次访问时间
            /// </summary>
            public DateTime? F_PreviousVisit { get; set; }

            /// <summary>
            /// 最后访问时间
            /// </summary>
            public DateTime? F_LastVisit { get; set; }

            /// <summary>
            /// 登录次数
            /// </summary>
            public int? F_LogOnCount { get; set; }

            /// <summary>
            /// 排序码
            /// </summary>
            public int? F_SortCode { get; set; }

            /// <summary>
            /// 删除标记
            /// </summary>
            public int? F_DeleteMark { get; set; }

            /// <summary>
            /// 有效标志
            /// </summary>
            public int? F_EnabledMark { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public string F_Description { get; set; }

            /// <summary>
            /// 创建日期
            /// </summary>
            public DateTime? F_CreateDate { get; set; }

            /// <summary>
            /// 创建用户主键
            /// </summary>
            public string F_CreateUserId { get; set; }

            /// <summary>
            /// 创建用户
            /// </summary>
            public string F_CreateUserName { get; set; }

            /// <summary>
            /// 修改日期
            /// </summary>
            public DateTime? F_ModifyDate { get; set; }

            /// <summary>
            /// 修改用户主键
            /// </summary>
            public string F_ModifyUserId { get; set; }

            /// <summary>
            /// 修改用户
            /// </summary>
            public string F_ModifyUserName { get; set; }
        }
    }

}
