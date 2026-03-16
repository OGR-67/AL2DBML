# Architecture

AL2DBML follows a Clean Architecture pattern: the CLI resolves dependencies through a dedicated Composition Root, while the Parser acts as infrastructure implementing contracts defined by the Application layer — keeping the Domain model free of any external dependency.

```mermaid
graph TD
    CLI["AL2DBML.CLI<br/><i>Presentation</i>"]
    DI["AL2DBML.DI<br/><i>Composition Root</i>"]
    APP["AL2DBML.Application<br/><i>Application</i>"]
    PARSER["AL2DBML.Parser<br/><i>Infrastructure</i>"]
    CORE["AL2DBML.Core<br/><i>Domain</i>"]
    TESTS["AL2DBML.Tests"]

    CLI --> DI
    CLI --> APP
    DI --> APP
    DI --> PARSER
    DI --> CORE
    APP --> CORE
    PARSER --> APP
    PARSER --> CORE
    TESTS --> APP
    TESTS --> PARSER
    TESTS --> CORE
    TESTS --> DI
```
