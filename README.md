# REST Query Language (RQL)
This project is a simple functional query language designed for use in REST API's.   It provides an abstraction from the actual data storage mechanisms used by the service.  So, whether you are using MongoDB, RavenDB, flat files or SQL, your API users can perform data queries in a standard, simple and consistent way.   Queries in RQL are mapped to their equivalent in the query language of the underlying database.  

Without RQL, or something like it, the only option is to pass strings from your REST API through to the database, thus exposing the underlying database you are using to your users.  This is also risky from a security standpoint as it is both a query injection and denial of service attack vector.   

For example, MongoDB allows the execution of Javascript directly on the database, and a database server can be effectively shut down by any number of poorly formed queries.  RQL provides a filter layer where both of these types of attacks can be prevented.

---

John Lyon-Smith, 2014.