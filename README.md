<h1 align="center">StartingMultiTenant</h1>

<div align="center">

A tenant database link management system based on multi - tenant independent database architecture

</div>

[![](./snap/tenants.png)](https://github.com/ZhiYuanHuang/StartingMultiTenant)

English | [简体中文](./README-zh_CN.md) 

## ✨ 特性

- ⚙️ A tenant is identified by a tenant domain (such as reader.com) and its unique identifier. That is, tenant A (tom.reader.com) and tenant B (tony.reader.com) belong to different tenant in the same tenant domains
- ⚙️ Databases such as postgresql and mysql are supported
- ⚙️ Database servers can be dynamically added to randomly select databases for creating all services provided by tenants
- ⚙️ The database creation script of the primary version and upgrade script of the secondary version are supported, for example, createTestdb.sql2.2 The database creation script of createTestDb whose primary version is 2 and secondary version is iterated to 2.
- ⚙️ Tenants can store internal and external database link strings. The internal link string is the database created through the database construction script of the system, and the external link string is only the link string maintained externally.
- ⚙️ Internal links have more maintenance functions, such as batch update database schema, update records, and so on
- ⚙️ The link string supports the service label serviceIdentifier and database label dbIdentifier.
- ⚙️ You can access tenant database link resources in the following ways:
    - http api 
    - redis, k8s Secret resource (Create vstore synchronous write)
- ⚙️ Support link string change push, using queue asynchronous push notification

## 📦 安装

1. Deploy the postgres database, and execute [StartingMultiTenant_Db_Sql](./StartingMultiTenant_Db_Sql/db.sql) script

```
psql.exe -U postgres -h localhost -p 5432 -f db.sql
```

2. Modifying the configuration file[appsettings.json](./Src/StartingMultiTenant/StartingMultiTenant.Api/appsettings.json),Modify configurations such as project database links

   ```
   {
     "SysAesKey": "startingmultiten",
     "StartingMultiTenantDbOption": {        //The project master database is linked to the slave database
       "MasterConnStr": "Host=127.0.0.1;Port=5433;Username=postgres;Password=123456;Database=startingmultitenant",
       "SlaveConnStr": "Host=127.0.0.1;Port=5433;Username=postgres;Password=123456;Database=startingmultitenant"
     },
     "QueueNotice": { //Change push queue Settings
       "Enable": true, //true:enable,false:disable

       "QueueType": "RabbitMQ",               //RabbitMQ:Push using rabbitmq, Redis: push using redis queue
       "QueueConn": "127.0.0.1;5673"          //Queue link string
      },
     //external store
     "ExternalStores": [
       {
         "StoreType": "Redis", //Redis: indicates redis external storage
         "Conn": "127.0.0.1:6379,password=123456"
       },
       {
        "StoreType": "k8s_secret", //k8s_secret: k8s Secret external store
        "ConfigFilePath": "./cer/kubeconfig", //kubeconfig file path
        "K8sNamespace": "dev" //the namespace of secret
       }
     ],
     //default setting
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

3. docker deploys the image or dotnet run runs

   docker image [zionyellow/startingmultitenant](https://hub.docker.com/repository/docker/zionyellow/startingmultitenant)
   ```bash
   docker run \
   -p 5251:80 \
   --name startingmultitenant \
   -v /root/docker/startmultitenant/appsettings.json:/app/appsettings.json \
   -v /root/docker/startingmultitenant/cer:/app/cer \
   -d zionyellow/startingmultitenant:1.0
   ```

   that '/app/cer' is the dir of kubeconfig file

4. visit ip:port/API/apiclient/init, initialize the administrator account

5. deploy the front project,please visit[StartingMultiTenant.front](https://github.com/ZhiYuanHuang/StartingMultiTenant.front)

## 🔨 example

### access an authorization token through the http api

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

### create tenant through the http api

```
curl --location --request POST 'http://localhost:5251/api/tenantcenter/create' \
--header 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2VydmljZUNsaWVudCIsInNjb3BlIjpbInJlYWQiLCJ3cml0ZSJdLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaXNzIjoiRkFOLklzc3VlciIsImF1ZCI6IkZBTi5BdWRpZW5jZSJ9.21oxggLD2PGfmzN9qFMvz_oekhPDMPzcPs7miimKLYk' \
--header 'Content-Type: application/json' \
--data-raw '{
    "data": {
        "tenantdomain": "test.com",     //domain of tenant
        "TenantIdentifier": "joicy2",   //identifier of tenant
        "tenantname": "test租户",       //Name of tenant, which can be empty
        "description": "testtest",      //Name of tenant, which can be empty
        "CreateDbScripts": [            //createdb list of tenant, which can be empty
            "CreateTestDb",             //The name of the database creation object is the name of the database creation script entered into the system
            "CreateOAuthDb"
        ]
    }
}'
```

### access tenant database link resources using http api

+ Tenant granularity to access database link resources

```
curl --location --request GET 'http://localhost:5251/api/tenantcenter/GetDbConn?tenantDomain=test.com&tenantIdentifier=joicy2' \
--header 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2VydmljZUNsaWVudCIsInNjb3BlIjpbInJlYWQiLCJ3cml0ZSJdLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaXNzIjoiRkFOLklzc3VlciIsImF1ZCI6IkZBTi5BdWRpZW5jZSJ9.21oxggLD2PGfmzN9qFMvz_oekhPDMPzcPs7miimKLYk'
```

+ Service granularity to access database link resources

```
curl --location --request GET 'http://localhost:5251/api/tenantcenter/GetDbConn?tenantDomain=test.com&tenantIdentifier=joicy2&serviceIdentifier=test.svc' \
--header 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2VydmljZUNsaWVudCIsInNjb3BlIjpbInJlYWQiLCJ3cml0ZSJdLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaXNzIjoiRkFOLklzc3VlciIsImF1ZCI6IkZBTi5BdWRpZW5jZSJ9.21oxggLD2PGfmzN9qFMvz_oekhPDMPzcPs7miimKLYk'
```

### The tenant database is linked to external storage

+ k8s Secret External Store (\$"{serviceIdentifier}-{dbIdentifier}-{(isInner?"inner":"external")}") as name，and content ($"{tenantDomain}_{tenantIdentifier}") as key,dbConn as value

![k8s Secret ](./snap/k8sSecrets.png)

![tenant dbconn in the Secret](./snap/k8sSecretDetail.png)

+ redis External Store

![redis store](./snap/redisStore.png)




