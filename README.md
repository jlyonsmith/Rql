# REST Query Language (RQL)

## Rationale

This project is a _simple_  query language designed for use in REST API's.  It provides an abstraction from the database used by the service.  Whether you are using MongoDB, RavenDB, flat files or SQL, your API users can perform data queries in a consistent, URL-safe way.  Queries in RQL are mapped to their equivalent in the query language of the underlying database, so you can expose as little or as much of it as you want to your users.

Without something like RQL you may be tempted to pass raw query strings from your REST API through to the database, thus exposing your underlying database to your users.  This is risky from a security standpoint as it is both a query injection and denial of service attack vector.

For example, MongoDB allows the execution of Javascript directly on the database, and a database server can be effectively shut down by any number of poorly formed queries.  RQL provides a filter layer where both of these types of attacks can be prevented or mitigated.

## Documentation

The project API and the MongoDB binding are documented in the [Wiki](https://github.com/jlyonsmith/Rql/wiki).

---

John Lyon-Smith, 2014.