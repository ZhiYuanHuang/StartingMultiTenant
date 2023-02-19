<h1 align="center">StartingMultiTenant</h1>

<div align="center">

一套基于多租户独立数据库架构的租户数据库链接管理系统

</div>

<!-- 租户页截图 -->

[English](./README.md) | 简体中文 

## ✨ 特性

- 🌈 租户由租户域（如reader.com），和该域下的唯一标识确定，即：租户A（tom.reader.com）和租户B（tony.reader.com）为不同租户域下的租户
- 📦 支持postgresql、mysql等类型的数据库
- 🛡 支持动态添加数据库服务器，随机选取创建租户数据库
- ⚙️ 支持主版本号的建库脚本，次版本号的升级脚本，如：createTestDb.sql_2.2为主版本号为2，次版本号迭代到2的createTestDb的建库脚本。
- 🌍 租户支持存储内部和外部两种数据库链接字符串，内部链接字符串为通过系统的建库脚本所创建出来的数据库，外部链接字符串仅为由外部维护的数据库、链接字符串写入系统。
- ⚙️ 内部字符串拥有更多维护的功能，如：批量升级数据库，升级记录等等
- 🎨 链接字符串支持服务标签 serviceIdentifier和数据库标签 dbIdentifier。
- 📦 获取租户数据库链接字符串，可通过接口拉取，同时系统支持变更推送（mq推送），和同步写入外部存储（redis，k8s的secret资源）
- ⚙️ 租户支持管理页面和接口创建，管理页面拥有更多可选项，接口创建仅支持创建不存在的租户

## 📦 安装

```bash
npm install antd --save
```

```bash
yarn add antd
```

## 🔨 示例

```jsx
import React from 'react';
import { Button, DatePicker } from 'antd';

const App = () => (
  <>
    <Button type="primary">PRESS ME</Button>
    <DatePicker />
  </>
);
```






