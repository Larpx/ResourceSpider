/*    此 SQL DDL 脚本由 Microsoft Visual Studio (发布日期:LOCAL BUILD) 生成。                            */

/*    所用驱动程序: Microsoft Visual Studio － Microsoft SQL Server 驱动程序。                            */
/*    文档: F:\MyProject\Spider\MovieSpider\Doc\MovieSpider V3.vsd。                             */
/*    创建时间: 2020年3月25日 3:30。                                                                  */
/*    操作: 来自 Visio 生成向导。                                                                      */
/*    已连接的数据源: 没有连接。                                                                          */
/*    已连接的服务器: 没有连接。                                                                          */
/*    已连接的数据库: 不适用。                                                                           */



SET QUOTED_IDENTIFIER ON

go


/* 创建 MovieSpider 数据库。                                                                        */
use master  

go

create database "MovieSpider"  

go

use "MovieSpider"  

go

/* 创建新表 "Setting"。                                                                            */
/* "Setting" : 系统设定                                                                           */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"Key" : 设置键                                                                               */
/* 	"Value" : 设置值                                                                             */
/* 	"Deleted" : 删除标记                                                                          */  
create table "Setting" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"Key" varchar(50) not null,
	"Value" nvarchar(max) not null,
	"Deleted" bit default 0 not null)  

go

alter table "Setting"
	add constraint "Setting_PK" primary key ("GUID")   


go

/* 创建新表 "Manager"。                                                                            */
/* "Manager" : 管理员                                                                            */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"UserName" : 用户名                                                                          */
/* 	"Password" : 密码                                                                           */
/* 	"Type" : 类型（0管理员 1商品内容审核 ）                                                                */
/* 	"Status" : 状态（0待审核 1正常 2冻结）                                                               */
/* 	"Name" : 真实姓名                                                                             */
/* 	"Mail" : 电子邮件                                                                             */
/* 	"Mobile" : 手机号                                                                            */
/* 	"Deleted" : 删除标记                                                                          */  
create table "Manager" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"UserName" nvarchar(50) not null,
	"Password" varchar(100) not null,
	"Type" tinyint default 0 not null,
	"Status" tinyint default 0 not null,
	"Name" nvarchar(20) null,
	"Mail" varchar(50) null,
	"Mobile" varchar(20) null,
	"Deleted" bit default 0 not null) ON 'PRIMARY'  

go

alter table "Manager"
	add constraint "Manager_PK" primary key nonclustered ("GUID")   


go

/* 创建新表 "SystemLog"。                                                                          */
/* "SystemLog" : 管理员日志                                                                        */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"ManagerGUID" : 管理员 GUID                                                                  */
/* 	"IP" : 登录 IP                                                                              */
/* 	"Date" : 发生时间                                                                             */
/* 	"Message" : 描述内容                                                                          */
/* 	"Module" : 操作模块（技术信息）                                                                     */
/* 	"Action" : 操作命令（技术信息）                                                                     */
/* 	"Result" : 操作结果（技术信息）                                                                     */
/* 	"Deleted" : 删除标记                                                                          */  
create table "SystemLog" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"ManagerGUID" uniqueidentifier not null,
	"IP" bigint default 0 not null,
	"Date" datetime default GETDATE() not null,
	"Message" nvarchar(max) not null,
	"Module" varchar(100) null,
	"Action" varchar(100) null,
	"Result" nvarchar(max) null,
	"Deleted" bit default 0 not null) ON 'PRIMARY'  

go

alter table "SystemLog"
	add constraint "SystemLog_PK" primary key nonclustered ("GUID")   


go

/* 创建新表 "Website"。                                                                            */
/* "Website" : 目标电商网站                                                                         */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"Date" : 日期                                                                               */
/* 	"URL" : URL                                                                               */
/* 	"Name" : 名称                                                                               */
/* 	"NameChs" : 中文名称                                                                          */
/* 	"Status" : 状态（0禁止 1正常 2下架）                                                                */
/* 	"Priority" : 优先级（0正常 >0优先级增加）                                                             */
/* 	"Processed" : 处理进度（0未处理 1处理中 2处理完成）                                                       */
/* 	"UpdateTime" : 更新时间                                                                       */
/* 	"UpdateTimes" : 更新次数                                                                      */
/* 	"Memo" : 备注                                                                               */
/* 	"Deleted" : 删除标记                                                                          */  
create table "Website" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"Date" datetime default GETDATE() not null,
	"URL" nvarchar(2048) not null,
	"Name" nvarchar(max) not null,
	"NameChs" nvarchar(max) null,
	"Status" tinyint default 0 not null,
	"Priority" tinyint default 0 not null,
	"Processed" tinyint default 0 not null,
	"UpdateTime" datetime default GETDATE() not null,
	"UpdateTimes" int default 0 not null,
	"Memo" nvarchar(max) null,
	"Deleted" bit default 0 not null)  

go

alter table "Website"
	add constraint "Website_PK" primary key ("GUID")   


go

/* 创建新表 "Category"。                                                                           */
/* "Category" : 商品分类                                                                          */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"WebsiteGUID" : 网站 GUID                                                                   */
/* 	"Name" : 名称                                                                               */
/* 	"NameChs" : 中文名称                                                                          */
/* 	"URL" : URL                                                                               */
/* 	"Date" : 日期                                                                               */
/* 	"Status" : 状态（0禁止 1正常 2下架）                                                                */
/* 	"Priority" : 优先级（0正常 >0优先级增加）                                                             */
/* 	"Processed" : 处理进度（0未处理 1处理中 2处理完成）                                                       */
/* 	"UpdateTime" : 更新时间                                                                       */
/* 	"UpdateTimes" : 更新次数                                                                      */
/* 	"Deleted" : 删除标记                                                                          */  
create table "Category" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"WebsiteGUID" uniqueidentifier not null,
	"Name" nvarchar(100) not null,
	"NameChs" nvarchar(100) null,
	"URL" nvarchar(max) null,
	"Date" datetime default GETDATE() not null,
	"Status" tinyint default 0 not null,
	"Priority" tinyint default 0 not null,
	"Processed" tinyint default 0 not null,
	"UpdateTime" datetime default GETDATE() not null,
	"UpdateTimes" int default 0 not null,
	"Deleted" bit default 0 not null)  

go

alter table "Category"
	add constraint "Category_PK" primary key ("GUID")   


go

/* 创建新表 "Link"。                                                                               */
/* "Link" : 资源链接                                                                              */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"WebsiteGUID" : 网站 GUID                                                                   */
/* 	"CategoryGUID" : 分类 GUID                                                                  */
/* 	"SN" : 链接编号（可重复）                                                                          */
/* 	"ID" : 系统编号（不可重复）                                                                         */
/* 	"URL" : URL                                                                               */
/* 	"Name" : 名称                                                                               */
/* 	"NameChs" : 中文名称                                                                          */
/* 	"Title" : 标题                                                                              */
/* 	"TitleChs" : 中文标题                                                                         */
/* 	"Brief" : 概要                                                                              */
/* 	"BriefChs" : 中文概要                                                                         */
/* 	"Detail" : 详情                                                                             */
/* 	"DetailChs" : 中文详情                                                                        */
/* 	"Date" : 日期                                                                               */
/* 	"Type" : 类型（0解析型 1下载型 2其他）                                                                */
/* 	"Processed" : 处理进度（0未处理 1处理中 2处理完成）                                                       */
/* 	"UpdateTime" : 更新时间                                                                       */
/* 	"UpdateTimes" : 更新次数                                                                      */
/* 	"Remark" : 备注                                                                             */
/* 	"Deleted" : 删除标记                                                                          */  
create table "Link" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"WebsiteGUID" uniqueidentifier not null,
	"CategoryGUID" uniqueidentifier null,
	"SN" varchar(100) not null,
	"ID" nvarchar(50) not null,
	"URL" nvarchar(max) not null,
	"Name" nvarchar(2048) null,
	"NameChs" nvarchar(2048) null,
	"Title" nvarchar(2000) null,
	"TitleChs" nvarchar(2000) null,
	"Brief" nvarchar(max) null,
	"BriefChs" nvarchar(max) null,
	"Detail" nvarchar(max) null,
	"DetailChs" nvarchar(max) null,
	"Date" datetime default GETDATE() not null,
	"Type" tinyint default 0 not null,
	"Processed" tinyint default 0 not null,
	"UpdateTime" datetime default GETDATE() not null,
	"UpdateTimes" int default 0 not null,
	"Remark" nvarchar(max) null,
	"Deleted" bit default 0 not null)  

go

alter table "Link"
	add constraint "Link_PK" primary key ("GUID")   


go

/* 创建新表 "PropertyKey"。                                                                        */
/* "PropertyKey" : 属性名                                                                        */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"Type" : 类型（0参数属性 1选项属性）                                                                  */
/* 	"Name" : 属性名                                                                              */
/* 	"NameChs" : 属性名（中文）                                                                       */
/* 	"Deleted" : 删除标记                                                                          */  
create table "PropertyKey" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"Type" tinyint default 0 not null,
	"Name" nvarchar(200) not null,
	"NameChs" nvarchar(200) null,
	"Deleted" bit default 0 not null)  

go

alter table "PropertyKey"
	add constraint "PropertyKey_PK" primary key ("GUID")   


go

/* 创建新表 "PropertyValue"。                                                                      */
/* "PropertyValue" : 属性值                                                                      */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"KeyGUID" : 属性名 GUID                                                                      */
/* 	"Type" : 类型（0参数值 1选项值）                                                                    */
/* 	"Name" : 属性值                                                                              */
/* 	"NameChs" : 属性值（中文）                                                                       */
/* 	"Deleted" : 删除标记                                                                          */  
create table "PropertyValue" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"KeyGUID" uniqueidentifier not null,
	"Type" tinyint default 0 not null,
	"Name" nvarchar(200) not null,
	"NameChs" nvarchar(200) null,
	"Deleted" bit default 0 not null)  

go

alter table "PropertyValue"
	add constraint "PropertyValue_PK" primary key ("GUID")   


go

/* 创建新表 "PropertyDetail"。                                                                     */
/* "PropertyDetail" : 属性详情                                                                    */
/* 	Key    Value                                                                              */
/* 	品牌   苹果                                                                                   */
/* 	CPU    A7                                                                                 */
/* 	内存   2G                                                                                   */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"LinkGUID" : 链接 GUID                                                                      */
/* 	"PropertyKeyGUID" : 属性名 GUID                                                              */
/* 	"KeyText" : 属性名                                                                           */
/* 	"PropertyValueGUID" : 属性值 GUID                                                            */
/* 	"ValueText" : 属性值                                                                         */
/* 	"Deleted" : 删除标记                                                                          */  
create table "PropertyDetail" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"LinkGUID" uniqueidentifier not null,
	"PropertyKeyGUID" uniqueidentifier not null,
	"KeyText" nvarchar(200) null,
	"PropertyValueGUID" uniqueidentifier null,
	"ValueText" nvarchar(200) null,
	"Deleted" bit default 0 not null)  

go

alter table "PropertyDetail"
	add constraint "PropertyDetail_PK" primary key ("GUID")   


go

/* 创建新表 "ResourceData"。                                                                       */
/* "ResourceData" : 链接资源                                                                      */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"WebsiteGUID" : 网站 GUID                                                                   */
/* 	"ObjectGUID" : 对象 GUID                                                                    */
/* 	"Date" : 日期                                                                               */
/* 	"URL" : URL                                                                               */
/* 	"Original" : 原文件名                                                                         */
/* 	"Name" : 链接文件名                                                                            */
/* 	"File" : 存储文件名                                                                            */
/* 	"URLType" : 连接类型（0URL 1ed2k 2magnet 3其他）                                                  */
/* 	"ResourceType" : 资源类型（0图片 1视频 2其他）                                                        */
/* 	"Status" : 状态（0禁止 1正常 2下架）                                                                */
/* 	"Password" : 链接密码（百度云）                                                                    */
/* 	"Size" : 文件大小                                                                             */
/* 	"Md5" : 文件MD5                                                                             */
/* 	"Hash" : 文件哈希值                                                                            */
/* 	"Processed" : 处理进度（0未处理 1处理中 2处理完成）                                                       */
/* 	"Memo" : 备注                                                                               */
/* 	"Deleted" : 删除标记                                                                          */  
create table "ResourceData" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"WebsiteGUID" uniqueidentifier not null,
	"ObjectGUID" uniqueidentifier not null,
	"Date" datetime default GETDATE() not null,
	"URL" nvarchar(2048) not null,
	"Original" nvarchar(2048) not null,
	"Name" nvarchar(2048) not null,
	"File" varchar(2048) not null,
	"URLType" tinyint not null,
	"ResourceType" tinyint default 0 not null,
	"Status" tinyint default 0 not null,
	"Password" varchar(100) null,
	"Size" bigint default 0 null,
	"Md5" nvarchar(128) null,
	"Hash" char(8) null,
	"Processed" tinyint default 0 not null,
	"Memo" nvarchar(max) null,
	"Deleted" bit default 0 not null) ON 'PRIMARY'  

go

alter table "ResourceData"
	add constraint "ResourceData_PK" primary key nonclustered ("GUID")   


go

/* 创建新表 "Resource"。                                                                           */
/* "Resource" : 网站资源                                                                          */
/* 	"GUID" : GUID 标识                                                                          */
/* 	"WebsiteGUID" : 网站 GUID                                                                   */
/* 	"PageGUID" : 页面 GUID                                                                      */
/* 	"URL" : 链接地址                                                                              */
/* 	"Original" : 原始文件名                                                                        */
/* 	"Date" : 日期                                                                               */
/* 	"Type" : 类型（0标题图 1内容图 2其他图）                                                               */
/* 	"Path" : 存储路径                                                                             */
/* 	"FileName" : 存储文件名                                                                        */
/* 	"Hash" : 文件哈希值 CRC32                                                                      */
/* 	"Deleted" : 删除标记                                                                          */  
create table "Resource" ( 
	"GUID" uniqueidentifier default NEWID() not null,
	"WebsiteGUID" uniqueidentifier not null,
	"PageGUID" uniqueidentifier not null,
	"URL" nvarchar(2048) null,
	"Original" nvarchar(2048) not null,
	"Date" datetime default GETDATE() not null,
	"Type" tinyint default 0 not null,
	"Path" varchar(max) not null,
	"FileName" varchar(2048) not null,
	"Hash" char(8) null,
	"Deleted" bit default 0 not null) ON 'PRIMARY'  

go

alter table "Resource"
	add constraint "Resource_PK" primary key nonclustered ("GUID")   


go

/* 在表 "SystemLog" 中添加外键约束。                                                                    */
alter table "SystemLog"
	add constraint "Manager_SystemLog_FK1" foreign key (
		"ManagerGUID")
	 references "Manager" (
		"GUID") on update no action on delete no action  

go

/* 在表 "Category" 中添加外键约束。                                                                     */
alter table "Category"
	add constraint "Website_Category_FK1" foreign key (
		"WebsiteGUID")
	 references "Website" (
		"GUID") on update no action on delete no action  

go

/* 在表 "Link" 中添加外键约束。                                                                         */
alter table "Link"
	add constraint "Website_Link_FK1" foreign key (
		"WebsiteGUID")
	 references "Website" (
		"GUID") on update no action on delete no action  

go

alter table "Link"
	add constraint "Category_Link_FK1" foreign key (
		"CategoryGUID")
	 references "Category" (
		"GUID") on update no action on delete no action  

go

/* 在表 "PropertyValue" 中添加外键约束。                                                                */
alter table "PropertyValue"
	add constraint "PropertyKey_PropertyValue_FK1" foreign key (
		"KeyGUID")
	 references "PropertyKey" (
		"GUID") on update no action on delete no action  

go

/* 在表 "PropertyDetail" 中添加外键约束。                                                               */
alter table "PropertyDetail"
	add constraint "Link_PropertyDetail_FK1" foreign key (
		"LinkGUID")
	 references "Link" (
		"GUID") on update no action on delete no action  

go

alter table "PropertyDetail"
	add constraint "PropertyKey_PropertyDetail_FK1" foreign key (
		"PropertyKeyGUID")
	 references "PropertyKey" (
		"GUID") on update no action on delete no action  

go

alter table "PropertyDetail"
	add constraint "PropertyValue_PropertyDetail_FK1" foreign key (
		"PropertyValueGUID")
	 references "PropertyValue" (
		"GUID") on update no action on delete no action  

go

/* 在表 "ResourceData" 中添加外键约束。                                                                 */
alter table "ResourceData"
	add constraint "Website_ResourceData_FK1" foreign key (
		"WebsiteGUID")
	 references "Website" (
		"GUID") on update no action on delete no action  

go

alter table "ResourceData"
	add constraint "Link_ResourceData_FK1" foreign key (
		"ObjectGUID")
	 references "Link" (
		"GUID") on update no action on delete no action  

go

/* 在表 "Resource" 中添加外键约束。                                                                     */
alter table "Resource"
	add constraint "Website_Resource_FK1" foreign key (
		"WebsiteGUID")
	 references "Website" (
		"GUID") on update no action on delete no action  

go

alter table "Resource"
	add constraint "Link_Resource_FK1" foreign key (
		"PageGUID")
	 references "Link" (
		"GUID") on update no action on delete no action  

go


/* 这是 Microsoft Visual Studio 生成的 SQL DDL 脚本的末尾。                                              */
