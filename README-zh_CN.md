<h1 align="center">StartingMultiTenant</h1>

<div align="center">

一套基于多租户独立数据库架构的租户数据库链接管理系统

</div>

[![](./snap/tenants.png)](https://github.com/ZhiYuanHuang/StartingMultiTenant)

[English](./README.md) | 简体中文 

## ✨ 特性

- ⚙️ 租户由租户域（如reader.com），和该域下的唯一标识确定，即：租户A（tom.reader.com）和租户B（tony.reader.com）为不同租户域下的租户
- ⚙️ 支持postgresql、mysql等类型的数据库
- ⚙️ 支持动态添加数据库服务器，随机选取创建租户所有服务的数据库
- ⚙️ 支持主版本号的建库脚本，次版本号的升级脚本，如：createTestDb.sql_2.2为主版本号为2，次版本号迭代到2的createTestDb的建库脚本。
- ⚙️ 租户支持存储内部和外部两种数据库链接字符串，内部链接字符串为通过系统的建库脚本所创建出来的数据库，外部链接字符串仅为由外部维护的链接字符串。
- ⚙️ 内部链接拥有更多维护的功能，如：批量升级数据库schema，升级记录等等
- ⚙️ 链接字符串支持服务标签 serviceIdentifier和数据库标签 dbIdentifier。
- ⚙️ 访问租户数据库链接资源的方式，有以下方式：
    - http api 
    - redis、k8s Secret资源（创建租户同步写入）
- ⚙️ 支持链接字符串变更推送，使用队列异步推送通知

## 📦 安装

1. 部署postgres数据库，执行[StartingMultiTenant_Db_Sql](./StartingMultiTenant_Db_Sql/db.sql)脚本

2. 修改配置文件[appsettings.json](./Src/StartingMultiTenant/StartingMultiTenant.Api/appsettings.json)，修改工程数据库链接等配置

   ```
   {
     "SysAesKey": "startingmultiten",
     "StartingMultiTenantDbOption": {        //工程主库与从库数据库链接
       "MasterConnStr": "Host=127.0.0.1;Port=5433;Username=postgres;Password=123456;Database=startingmultitenant",
       "SlaveConnStr": "Host=127.0.0.1;Port=5433;Username=postgres;Password=123456;Database=startingmultitenant"
     },
     "QueueNotice": { //变更推送队列设置
       "Enable": true, //true:启用，false:禁用

       "QueueType": "RabbitMQ",               //RabbitMQ:使用rabbitmq推送，Redis：使用redis队列推送
       "QueueConn": "127.0.0.1;5673"          //队列链接字符串
      },
     //外部存储
     "ExternalStores": [
       {
         "StoreType": "Redis", //Redis：redis类型外部存储，
         "Conn": "127.0.0.1:6379,password=123456"
       },
       {
        "StoreType": "k8s_secret", //k8s_secret: k8s Secret 外部存储
        "ConfigFilePath": "./cer/kubeconfig", //kubeconfig文件路径
        "K8sNamespace": "dev" //写入Secret的命名空间
       }
     ],
     //默认设置
     "JwtTokenOptions": {
       "Issuer": "FAN.Issuer",
       "ValidateIssuer": true,
       "Audience": "FAN.Audience",
       "ValidateAudience": true,
       "RawSigningKey": "11111111-1111-1111-1111-111111111111", /*签名秘钥*/
       "ValidateIssuerSigningKey": true
     },
     "Logging": {
       "LogLevel": {
       "Default": "Information",
       "Microsoft.AspNetCore": "Warning"
     }
    },
    "AllowedHosts": "*"
   }
   ```

3. docker部署镜像，或者dotnet run运行

   docker 镜像 [startingmultitenant]()
   ```bash
   docker run -p 5251:80 --name startingmultitenant -v /root/docker/startmultitenant/appsettings.json:/app/appsettings.json -d startingmultitenant:1.0
   ```

4. 部署前端工程，请转[StartingMultiTenant.front](https://github.com/ZhiYuanHuang/StartingMultiTenant.front)

## 🔨 示例

### 通过 http api 获取授权token

```
curl --location --request POST 'http://localhost:5251/api/connect/token' \
--header 'Content-Type: application/json' \
--data-raw '{
    "data": {
        "clientid": "serviceClient",
        "clientsecret": "123456"
    }
}'
```

### 通过 http api 创建租户

```
curl --location --request POST 'http://localhost:5251/api/tenantcenter/create' \
--header 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2VydmljZUNsaWVudCIsInNjb3BlIjpbInJlYWQiLCJ3cml0ZSJdLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaXNzIjoiRkFOLklzc3VlciIsImF1ZCI6IkZBTi5BdWRpZW5jZSJ9.21oxggLD2PGfmzN9qFMvz_oekhPDMPzcPs7miimKLYk' \
--header 'Content-Type: application/json' \
--data-raw '{
    "data": {
        "tenantdomain": "test.com",     //租户域
        "TenantIdentifier": "joicy2",   //租户标识
        "tenantname": "test租户",       //名称，可为空
        "description": "testtest",      //描述，可为空
        "CreateDbScripts": [            //创建库列表，可为空
            "CreateTestDb",             //建库对象名称，为录入系统的建库脚本名称
            "CreateOAuthDb"
        ]
    }
}'
```

### 通过 http api 获取租户数据库链接资源

```
curl --location --request GET 'http://localhost:5251/api/tenantcenter/GetDbConn?tenantDomain=test.com&tenantIdentifier=joicy2' \
--header 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2VydmljZUNsaWVudCIsInNjb3BlIjpbInJlYWQiLCJ3cml0ZSJdLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaXNzIjoiRkFOLklzc3VlciIsImF1ZCI6IkZBTi5BdWRpZW5jZSJ9.21oxggLD2PGfmzN9qFMvz_oekhPDMPzcPs7miimKLYk'
```





