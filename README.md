# 📚 ASP.NET-Core-Arch-Template
Plug and play template for the creation of a mid-scale ASP.NET Web projects. The template provides architecture that utilizes the best architectural practices and coding guidelines in the field, but yet the architecture is easy enough to be understood and extended.

This project follows a Multitier architecture designed for scalability, maintainability, and testability. The solution is structured into logical tiers, separating concerns across Web, Services, Data, Tests and Common modules. Each logical tier follows a minified Multilevel architecture, allowing for further seperation of the set of concerns down to the Single Responsibility of a layer, thus improving design efficiency and flexibility. 

---

## 🚀 Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/KrIsKa7a/ASP.NET-Core-Arch-Template.git
   ```

2. Navigate to the project directory:
   ```bash
   cd ASP.NET-Core-Arch-Template
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to:
   ```
   http://localhost:{defaultAppPort}
   ```

---


## 🛠️ Development Principles

- **Separation of concerns**: Each layer has a single responsibility.
- **Inversion of Control**: All dependencies are injected for maximum flexibility. This allows the developer to take control over some of the fundamental functionalities, provided by the ASP.NET Core Web Framework.
- **Modular Design**: New features can be added with minimal impact on existing code.
- **Testing First**: The template provides seperate layers for performing different types of testing - Blackbox, Whitebox, Sandbox.

---

## 🏛️ Architectural Tiers

The project architecture is divided into five main tiers, each responsible for a specific set of concerns. Each of these tiers is designed to be independently extendable and can be executed on separate hosts, significantly reducing the server’s overall load. Communication between tiers is handled through a Contract-based mechanism, enabling a high level of abstraction and flexibility regarding where each tier is deployed. Furthermore, each tier is represented by a minified Multilevel layered architecture, additionally increasing the level of abstraction of the lower level software, which is often provided by frameworks, middlewares or third-party libraries. 

---

### 1. Application/Presentation Tier

📦 **Projects:**
- `Web`
- `Web.ViewModels`
- `Web.Infrastructure`

🎯 **Purpose:**
- Acts as the entry point for users and external systems.
- Handles HTTP requests and responses.
- Prepares data for display in the current UI context.
- Provides filters, middleware, and user interaction logic.
- Provides the naming context and IoC Container to the neighbour tiers.

🔗 **Dependencies:**
- Communicates with the **Services Tier** through injected services.
- Communicates with the **Data Tier** to enable **Inversion of Control** and **AutoMapping**.
- Uses the data in the **Shared Tier**

📝 **Notes:**
- The developer and the future architect is free to extend the architecture with multiple **Application/Presentation Tiers**, while reusing the functionality of the other **Tiers**. Several **Application/Presentation Tiers** can be running simultaneously, because of the different naming contexts provided to the other tiers. Such architectural template allows for great scaleability and extendibility. 

---

## 2. Services Tier

📦 **Projects:**
- `Services.Core`
- `Services.AutoMapping`
- `Services.Common`

🎯 **Purpose:**
- Contains the core application logic and business rules.
- Orchestrates workflows between **Presentation and Data tiers**.
- Maps entity models to view models and vice versa via AutoMapper.
- Provides reusable service components and helpers.

🔗 **Dependencies:**
- Calls the **Data Tier** for persistence/retrieval operations.
- Uses the data in the **Shared Tier**

📝 **Notes:**
- The architectural template is supposed to use a single **Service Tier**, that may be shared between different **Application/Presentation Tiers**.
- In some cases, the size of the **Service Tier** may go above the acceptable limits. In these cases, the **Service Tier** may be splitted internally using the internal Multilevel layered architecture.
- The same **Service Tier** containing common business functionality can be used by different **Application/Presentation Tiers** (for example a Web, Android and iOS applications may use the same bussiness logic). The architectural template is configured to support **Inversion of Control** principle, thus allowing the same **Service Tier** to be used with different **Data Tiers** (DB Providers). This demonstrates the reusability and the extendibility of the proposed template.
---

## 3. Data Tier

📦 **Projects:**
- `Data`
- `Data.Models`
- `Data.Common`

🎯 **Purpose:**
- Manages database access through different Database Providers.
- Defines and maintains database models/entities.
- Implements repository patterns, specifications, unit of works, seeders and database utilities.
- Abstracts away the underlying database technology from upper layers.

🔗 **Dependencies:**
- The only dependency of the **Data Tier** lies in the used Database Provider and Database Server.
- The **Data Tier** is intended to provide high level of abstraction on the used Database Provider and Database Server to the **Service Tier**.
- Uses the data in the **Shared Tier**.

📝 **Notes:**
- Usually a single **Data Tier** is used to abstract on a single Database Provider and Database Server used by the application.
- In some cases, different **Application/Presentation Tiers** or even a single **Application/Presentation Tier** may need to access different Database Providers and Database Servers. In these cases it is up to the future architect of the application to decide between the following concepts:
  * Concept 1 - Usage of multiple DbContexts within a single **Data Tier**. This concept is suitable in the cases when different Database Provider and Database Server needs to be used, but the logic of the abstraction remains unchanged.
  * Concept 2 - Usage of multiple **Data Tiers**, which represent different Database Provider and Database Server. This concepts allows for customization of the abstraction and common functionality, provided by each of the used **Data Tiers**. However, this concept leads to significant architectural impacts and needs to be analyzed carefully before used.
---

## 4. Cross-Cutting Tier

📦 **Projects:**
- `GCommon`

🎯 **Purpose:**
- Provides utilities, constants, enums, global helpers that are needed across multiple tiers.
- Introduces cross-cutting concerns like logging, validation, error handling where needed.

🔗 **Dependencies:**
- Shared across all other tiers.
- Promotes reusability and consistent behavior across the application.

📝 **Notes:**
- This tier is intended to provide shared data and functionality among all of the other **Tiers**.
- The **Cross-Cutting Tier** may be referenced by any other **Tier**, even by newly introduced **Tiers** by the future architects and developers.
- Shared libraries and functionalities may also be stored in the **Cross-Cutting Tier**.

---
## 5. Test Tier

📦 **Projects:**
- `Web.Tests`
- `Services.Core.Tests`
- `Integration Tests`

🎯 **Purpose:**
- Contains unit and integration tests ensuring the quality of different layers.
- Performing Blackbox testing of the business logic of the application.
- Performing Whitebox testing of the distributed features of the application.
- Provides environment for performing isolated Sandbox testing.
- Performing Quality of Service (QoS) testing by means of a Contract-based mechanism.

🔗 **Dependencies:**
- References each of the **Tiers** that needs to be tested.
- Relies on the usage of an already implemented Test framework. Currently [**NUnit**](https://nunit.org/) is used in the template, but the developer is free to use any Test framework preferred (e.g. xUnit, MSTest, etc).

---

# 🧩 Solution Structure

| Layer                   | Purpose                                                                 |
|--------------------------|------------------------------------------------------------------------|
| `Web`                    | Entry point for the application (MVC/WebAPI controllers, middleware)  |
| `Web.ViewModels`         | Classes to represent the data in the application UI                         |
| `Web.Infrastructure`     | Common utilities for the Web project (e.g., extensions, filters, custom middlewares)       |
| `Web.Tests`              | Unit tests for the Web layer                                           |
| `Services.Core`          | Business logic and application services                               |
| `Services.AutoMapping`   | AutoMapper configuration and extensions to be used in the **Service Tier**                         |
| `Services.Common`        | Shared service helpers and utilities (e.g., validation, extensions)   |
| `Services.Core.Tests`    | Unit tests for the business logic                                       |
| `Data`                   | Database access layer (Repositories, DbContext, Seeding, FluentAPI)                      |
| `Data.Models`            | Code-first description of the Entities                                |
| `Data.Common`            | Base repositories, specifications, base models, database utilities                 |
| `GCommon`          | Cross-cutting concerns (e.g., shared constants, enums, global helpers)        |
| `Integration Tests`      | End-to-end testing across layers                                       |

The individual layers are represented by individual folder in the file structure of the template. The intention behind this decision is that each of the layers should be added as a submodule of the superproject repository. This allows for smoother execution of the integration process, improves versioning and contributes to the CI/CD of the application. The current architectural template is not splitted into submodules, because it is intended to be used as a reference for building ASP.NET Core Web Apps by C# students. We did not want to add further complications into the template and decided to leave the Git submodules approach as a suggestion to the future user of the template. 

On the other side, the lack of grouping of individual layers into **Tier** folders leads to confusion while developing the application. To deal with this, a Solution Folders are introduced for the different **Tiers** of the architecture.

---
## 📈 UML Diagram
A visual representation of the Multitier architecture of the template and the relations of each **Tier** are shown in the following UML diagrams:

![image](https://github.com/user-attachments/assets/badb5b27-f719-4d2f-afb4-8e409e9af13b)
![image](https://github.com/user-attachments/assets/43fec98f-9890-45c2-b993-0831f8384cc5)



---
## 🤝 Contributing

We welcome contributions!  
Please follow the coding conventions and submit PRs with clear descriptions.

---

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/KrIsKa7a/ASP.NET-Core-Arch-Template/blob/main/LICENSE) file for details.

---
