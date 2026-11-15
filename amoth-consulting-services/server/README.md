# Amoth Consulting Services - Backend API

A Fastify-based REST API backend for the Amoth Consulting Services application, providing authentication, user management, invoicing, project management, task tracking, and meeting scheduling.

## Tech Stack

- **Framework**: Fastify 4
- **Database**: PostgreSQL
- **Authentication**: JWT + bcryptjs
- **CORS**: Enabled for frontend integration
- **Node.js**: v24.x (required)

## Project Structure

```
server/
├── src/
│   ├── database/
│   │   ├── init.js          # Database initialization script
│   │   └── db.js            # Database connection management
│   ├── middleware/
│   │   └── auth.js          # JWT authentication middleware
│   ├── routes/
│   │   ├── auth.js          # Authentication endpoints
│   │   ├── profile.js       # User profile endpoints
│   │   ├── invoices.js      # Invoice management endpoints
│   │   ├── projects.js      # Project management endpoints
│   │   ├── tasks.js         # Task management endpoints
│   │   └── meetings.js      # Meeting scheduling endpoints
│   ├── utils/
│   │   └── auth.js          # Authentication utilities
│   └── server.js            # Main Fastify application
├── package.json
├── .env.example
└── README.md
```

## Setup Instructions

### 1. Install Dependencies

```bash
cd server
npm ci
```

### 2. Create Environment File

```bash
cp .env.example .env
```

Edit `.env` and customize if needed:

```
PORT=3001
NODE_ENV=development
JWT_SECRET=your-secret-key-change-this-in-production
MFA_ENCRYPTION_KEY=64-character-hex-key-change-this-in-production
CORS_ORIGIN=http://localhost:5173
FRONTEND_URL=http://localhost:5173
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USER=your-smtp-username
SMTP_PASSWORD=your-smtp-password
SMTP_FROM=Amoth Consulting <no-reply@example.com>
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=amoth
DATABASE_USERNAME=postgres
DATABASE_PASSWORD=your-postgres-password
DATABASE_SSL=false
DATABASE_SSL_REJECT_UNAUTHORIZED=true
```

For production, set a randomly generated `JWT_SECRET` of at least 32 characters, set `CORS_ORIGIN` to the deployed frontend URL, and use `DATABASE_SSL=true` with certificate verification enabled.
Set `MFA_ENCRYPTION_KEY` to a randomly generated 32-byte key encoded as 64 hexadecimal characters. Store it with the other production secrets.

Create the PostgreSQL database before initialization:

```sql
CREATE DATABASE amoth;
```

### 3. Initialize Database

```bash
npm run init-db
```

This creates the PostgreSQL tables and seeds the demo user and service content.

For local development only, set `SEED_DEMO_USER=true` and optionally `DEMO_PASSWORD` before running the initializer. Demo users are never created by default or in production.

### 4. Start the Server

**Development Mode (with auto-reload):**
```bash
npm run dev
```

**Production Mode:**
```bash
npm start
```

The server will start at `http://localhost:3001`

## API Endpoints

### Authentication (Public)

Cookie-authenticated state-changing requests must include the `X-CSRF-Token` header matching the non-HttpOnly `csrf` cookie. The frontend client handles this automatically. Bearer-token API clients are exempt from the cookie CSRF check.

#### Register User
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "string",
  "email": "string",
  "password": "string",
  "fullName": "string"
}

Response: 201
{
  "message": "User registered successfully",
  "token": "jwt_token",
  "user": { id, username, email, fullName }
}
```

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}

Response: 200
{
  "message": "Login successful",
  "user": { id, username, email, fullName, bio }
}
```

If MFA is enabled, login without a valid six-digit `mfaToken` returns `401` with `mfaRequired: true`.

#### Password Reset

Configure `SMTP_HOST`, `SMTP_PORT`, and `SMTP_FROM` (and SMTP credentials when required) plus `FRONTEND_URL`. Reset requests send a one-hour, single-use link and return the same response whether or not the email matches an account.

```
POST /api/auth/password-reset/request
{ "email": "user@example.com" }

POST /api/auth/password-reset/confirm
{ "token": "token-from-email-link", "password": "new-password-at-least-12-characters" }
```

The database migration creates the reset-token table automatically when the server starts.

#### MFA Setup
```
POST /api/auth/mfa/setup
POST /api/auth/mfa/enable
```

Use the setup response URI with an authenticator app, then send its six-digit code to the enable endpoint. MFA secrets are encrypted before storage.

### Profile (Protected)

All profile endpoints require `Authorization: Bearer <token>` header.

#### Get Profile
```
GET /api/profile
Response: 200
{
  "id": number,
  "username": "string",
  "email": "string",
  "fullName": "string",
  "bio": "string",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

#### Update Profile
```
PUT /api/profile
Content-Type: application/json

{
  "email": "string (optional)",
  "fullName": "string (optional)",
  "bio": "string (optional)"
}

Response: 200
{
  "message": "Profile updated successfully",
  "user": { ... }
}
```

#### Change Password
```
POST /api/change-password
Content-Type: application/json

{
  "currentPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}

Response: 200
{
  "message": "Password changed successfully"
}
```

### Invoices (Protected)

#### Get All Invoices
```
GET /api/invoices
Response: 200
[
  {
    "id": number,
    "invoiceNumber": "string",
    "amount": number,
    "dueDate": "date",
    "description": "string",
    "status": "Pending|Paid|Overdue",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
]
```

#### Get Invoice
```
GET /api/invoices/:id
Response: 200 { invoice object }
```

#### Create Invoice
```
POST /api/invoices
Content-Type: application/json

{
  "invoiceNumber": "string",
  "amount": number,
  "dueDate": "date (YYYY-MM-DD)",
  "description": "string (optional)"
}

Response: 201
{
  "message": "Invoice created successfully",
  "invoice": { ... }
}
```

#### Update Invoice
```
PUT /api/invoices/:id
Content-Type: application/json

{
  "status": "string (optional)",
  "amount": number (optional),
  "dueDate": "date (optional)",
  "description": "string (optional)"
}

Response: 200
{
  "message": "Invoice updated successfully",
  "invoice": { ... }
}
```

#### Delete Invoice
```
DELETE /api/invoices/:id
Response: 200
{
  "message": "Invoice deleted successfully"
}
```

### Projects (Protected)

#### Get All Projects
```
GET /api/projects
Response: 200
[
  {
    "id": number,
    "name": "string",
    "description": "string",
    "status": "Active|Completed|On Hold",
    "completionRate": number,
    "startDate": "date",
    "endDate": "date",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
]
```

#### Get Project with Tasks
```
GET /api/projects/:id
Response: 200
{
  ...project,
  "tasks": [ { task objects } ]
}
```

#### Create Project
```
POST /api/projects
Content-Type: application/json

{
  "name": "string",
  "description": "string (optional)",
  "status": "string (optional, default: Active)",
  "startDate": "date (optional)",
  "endDate": "date (optional)"
}

Response: 201
{
  "message": "Project created successfully",
  "project": { ... }
}
```

#### Update Project
```
PUT /api/projects/:id
Content-Type: application/json

{
  "name": "string (optional)",
  "description": "string (optional)",
  "status": "string (optional)",
  "completionRate": number (optional),
  "startDate": "date (optional)",
  "endDate": "date (optional)"
}

Response: 200
{
  "message": "Project updated successfully",
  "project": { ... }
}
```

#### Delete Project (and associated tasks)
```
DELETE /api/projects/:id
Response: 200
{
  "message": "Project deleted successfully"
}
```

### Tasks (Protected)

#### Get All Tasks (optionally filtered by project)
```
GET /api/tasks?projectId=1
Response: 200
[
  {
    "id": number,
    "title": "string",
    "description": "string",
    "projectId": number|null,
    "status": "Pending|In Progress|Completed",
    "dueDate": "date",
    "completed": boolean,
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
]
```

#### Get Task
```
GET /api/tasks/:id
Response: 200 { task object }
```

#### Create Task
```
POST /api/tasks
Content-Type: application/json

{
  "title": "string",
  "description": "string (optional)",
  "projectId": number (optional),
  "status": "string (optional, default: Pending)",
  "dueDate": "date (optional)"
}

Response: 201
{
  "message": "Task created successfully",
  "task": { ... }
}
```

#### Update Task
```
PUT /api/tasks/:id
Content-Type: application/json

{
  "title": "string (optional)",
  "description": "string (optional)",
  "status": "string (optional)",
  "dueDate": "date (optional)",
  "completed": boolean (optional)
}

Response: 200
{
  "message": "Task updated successfully",
  "task": { ... }
}
```

#### Delete Task
```
DELETE /api/tasks/:id
Response: 200
{
  "message": "Task deleted successfully"
}
```

### Meetings (Protected)

#### Get All Meetings
```
GET /api/meetings
Response: 200
[
  {
    "id": number,
    "title": "string",
    "description": "string",
    "scheduledDate": "datetime",
    "duration": number (minutes),
    "status": "Scheduled|Completed|Cancelled",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
]
```

#### Get Meeting
```
GET /api/meetings/:id
Response: 200 { meeting object }
```

#### Create Meeting
```
POST /api/meetings
Content-Type: application/json

{
  "title": "string",
  "description": "string (optional)",
  "scheduledDate": "datetime (ISO format)",
  "duration": number (optional, default: 60),
  "status": "string (optional, default: Scheduled)"
}

Response: 201
{
  "message": "Meeting created successfully",
  "meeting": { ... }
}
```

#### Update Meeting
```
PUT /api/meetings/:id
Content-Type: application/json

{
  "title": "string (optional)",
  "description": "string (optional)",
  "scheduledDate": "datetime (optional)",
  "duration": number (optional)",
  "status": "string (optional)"
}

Response: 200
{
  "message": "Meeting updated successfully",
  "meeting": { ... }
}
```

#### Delete Meeting
```
DELETE /api/meetings/:id
Response: 200
{
  "message": "Meeting deleted successfully"
}
```

## Error Handling

All endpoints return appropriate HTTP status codes:

- **200**: Success
- **201**: Created
- **400**: Bad Request (validation error)
- **401**: Unauthorized (missing or invalid token)
- **404**: Not Found
- **409**: Conflict (duplicate resource)
- **500**: Server Error

Error responses follow this format:
```json
{
  "error": "Error message"
}
```

## Database Schema

### Users Table
- `id` (Integer, Primary Key)
- `username` (Text, Unique)
- `email` (Text, Unique)
- `password` (Text, hashed)
- `fullName` (Text)
- `bio` (Text)
- `createdAt` (DateTime)
- `updatedAt` (DateTime)

### Invoices Table
- `id` (Integer, Primary Key)
- `invoiceNumber` (Text, Unique)
- `userId` (Integer, Foreign Key)
- `amount` (Real)
- `dueDate` (Date)
- `description` (Text)
- `status` (Text)
- `createdAt` (DateTime)
- `updatedAt` (DateTime)

### Projects Table
- `id` (Integer, Primary Key)
- `name` (Text)
- `description` (Text)
- `userId` (Integer, Foreign Key)
- `status` (Text)
- `completionRate` (Real)
- `startDate` (Date)
- `endDate` (Date)
- `createdAt` (DateTime)
- `updatedAt` (DateTime)

### Tasks Table
- `id` (Integer, Primary Key)
- `title` (Text)
- `description` (Text)
- `projectId` (Integer, Foreign Key, nullable)
- `userId` (Integer, Foreign Key)
- `status` (Text)
- `dueDate` (Date)
- `completed` (Boolean)
- `createdAt` (DateTime)
- `updatedAt` (DateTime)

### Meetings Table
- `id` (Integer, Primary Key)
- `title` (Text)
- `description` (Text)
- `userId` (Integer, Foreign Key)
- `scheduledDate` (DateTime)
- `duration` (Integer)
- `status` (Text)
- `createdAt` (DateTime)
- `updatedAt` (DateTime)

## Usage Example (with fetch)

```javascript
// Register
const registerResponse = await fetch('http://localhost:3001/api/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'newuser',
    email: 'user@example.com',
    password: 'securepassword',
    fullName: 'New User'
  })
});
const { token } = await registerResponse.json();

// Get Profile
const profileResponse = await fetch('http://localhost:3001/api/profile', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const profile = await profileResponse.json();

// Create Invoice
const invoiceResponse = await fetch('http://localhost:3001/api/invoices', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    invoiceNumber: 'INV-2024-001',
    amount: 1500,
    dueDate: '2024-07-15',
    description: 'Consulting Services'
  })
});
const { invoice } = await invoiceResponse.json();
```

## Security Considerations

1. **JWT Secret**: Change the `JWT_SECRET` in `.env` for production
2. **Password Hashing**: Passwords are hashed using bcryptjs (10 rounds)
3. **CORS**: Configured to accept requests from any origin (modify as needed)
4. **Database**: Use absolute paths or secure database credentials for production
5. **Environment Variables**: Never commit `.env` file to version control

## Development Notes

- Demo user is created on first database initialization
- All timestamps are stored in UTC
- All API endpoints use pagination-ready structure (can be added if needed)
- Database transactions can be added for complex operations

## Support

For issues or questions, please refer to the main project README or contact the development team.
