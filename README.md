
# Fortunae Library Management System API

## Overview
The **Fortunae Library Management System API** is a backend service built using **ASP.NET Core** with **Clean Architecture** principles. It provides a robust and scalable foundation for managing library resources, including books, users, and borrowing activities.

The API supports JWT-based authentication and role-based access control, ensuring secure interactions between the system and users.

---

## Features

### 1. Authentication & Authorization
- Uses **JWT-based authentication** for secure access.
- Implements **role-based access control**:
  - **Admin**: Can manage books, users, and borrowing activities.
  - **Member**: Can browse books, borrow and return books, and view borrowing history.

### 2. Book Management
- **Admin Privileges:**
  - Add, update, and delete books.
  - Manage book details such as title, author, genre, ISBN, and availability.
- **Member Privileges:**
  - View a list of available books.
  - Search and filter books by title, author, genre, or availability.

### 3. User Management
- **Admin Privileges:**
  - View a list of all registered users.
  - Manage user roles (e.g., promote a user to admin).

### 4. Borrowing System
- **Member Privileges:**
  - Borrow books (up to **3 books at a time**).
  - Return borrowed books.
- **Admin Privileges:**
  - View all borrowing activities.
  - Mark overdue books as returned.
  - Penalize members for delayed returns.

---

## Architecture
This project follows **Clean Architecture** principles, separating concerns into distinct layers for maintainability and scalability.

### Folder Structure:
```
LibraryManagementSystem/
├── src/
│   ├── Application/          # Business logic (Interfaces, Services, DTOs)
│   ├── Domain/               # Core domain logic (Entities, Value Objects, Exceptions)
│   ├── Infrastructure/       # Database, Repositories, Security, Logging
│   ├── Presentation/         # API Controllers, Middleware, Filters
├── tests/
│   ├── UnitTests/            # Unit testing for services and components
│   ├── IntegrationTests/     # End-to-end API testing
├── LibraryManagementSystem.sln # Solution file
```

---

## Installation & Setup
### Prerequisites
Ensure you have the following installed:
- **.NET 7 SDK** or later
- **SQL Server** or any other configured database
- **Postman** (optional, for API testing)

### Steps to Run the Application
1. **Clone the repository:**
   ```sh
   git clone https://github.com/aristokratos/FortunaeLibraryManagementSystemApi.git
   cd FortunaeLibraryManagementSystemApi
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Update database connection string:**
   - Modify `appsettings.json` to configure your database connection.
4. **Apply database migrations:**
   ```sh
   dotnet ef database update
   ```
5. **Run the application:**
   ```sh
   dotnet run
   ```
6. **Access API via Swagger UI:**
   - Navigate to `http://localhost:<PORT>/swagger` to explore API endpoints.

---

## API Endpoints
### Authentication
| Method | Endpoint           | Description |
|--------|-------------------|-------------|
| POST   | `/api/auth/login`  | Login and obtain JWT token |
| POST   | `/api/auth/register` | Register a new user |

### Book Management
| Method | Endpoint                 | Access  | Description |
|--------|-------------------------|---------|-------------|
| GET    | `/api/books`             | Public  | Get all books |
| GET    | `/api/books/{id}`        | Public  | Get book details |
| POST   | `/api/books`             | Admin   | Add a new book |
| PUT    | `/api/books/{id}`        | Admin   | Update book details |
| DELETE | `/api/books/{id}`        | Admin   | Delete a book |

### User Management
| Method | Endpoint            | Access  | Description |
|--------|--------------------|---------|-------------|
| GET    | `/api/users`       | Admin   | Get all users |
| PUT    | `/api/users/{id}`  | Admin   | Update user role |

### Borrowing System
| Method | Endpoint               | Access  | Description |
|--------|-----------------------|---------|-------------|
| POST   | `/api/borrow/{bookId}` | Member  | Borrow a book |
| POST   | `/api/return/{bookId}` | Member  | Return a borrowed book |
| GET    | `/api/borrow/history`  | Member  | View borrowing history |
| GET    | `/api/borrow/all`      | Admin   | View all borrowing records |

---

## Testing
This project includes **unit tests** and **integration tests** to ensure API reliability.
### Running Tests:
```sh
dotnet test
```

---

## Contributing
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -m "Added new feature"`).
4. Push to the branch (`git push origin feature-branch`).
5. Open a pull request.

---

## License
This project is licensed under the **MIT License**. See the `LICENSE` file for details.

---

## Contact
For any inquiries or suggestions, feel free to reach out via [GitHub Issues](https://github.com/aristokratos/FortunaeLibraryManagementSystemApi/issues).

