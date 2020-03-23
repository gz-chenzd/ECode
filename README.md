# ECode
ECode是一套NetCore组件，提供了一套面向SaaS的ORM框架及各种常用的基础类库，包括缓存Redis/Memcached、数据校验CRC/哈希/RSA、数据编码B64/QP、加解密AES/DES、依赖注入DI/IoC、事件框架、日志组件、抽象配置及部分常用工具方法，用于帮助大家简化系统开发，减少系统臃肿

# Core
Core是ECode的核心组件，包含有各种常用的基础类库，用于帮助大家简化系统开发

- 缓存： 统一的接口及多个缓存实现 内存缓存、Redis Kv及Memcached
- 数据校验： 收录了大部分常用的CRC、MD5、SHA及RSA签名验证等
- 数据编码： 收录有Base64、QuotedPrintable、Hex、UrlEncode等
- 加解密： 统一的接口及其实现AES、DES
- 依赖注入： 一套小巧精致的依赖注入实现
- 事件框架： 一套程序内的事件通知机制实现
- 日志组件： 类似与Log4Net的实现，主要用于自定义监控使用
- 配置管理： 抽象的配置管理，用于管理基于环境变量及程序自身的配置
- 其他常用方法： 如类型转换、参数校验、网络调用等等

# Data
Data是ECode的ORM组件部分，提供了基于lambda表达式同时又类似于SQL语法的使用模式，支持多种关系型数据库MySQL、MS SQL Server、SQLite等

- 支持分库分表： 基于策略接口的、自定义规则的分库分表实现
- 支持读写分离： 自动根据SQL上下文进行主从选择自由切换
- 支持事务： 支持基于ReadCommitted隔离级别的事务
- 支持MySQL、MsSQL、SQLite等多种数据库

<br />
<br />
QQ群： 10527852
