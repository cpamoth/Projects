# Quick Start Guide - Full Stack Setup

This project consists of:
1. **Frontend**: Vue 3 + Vite (client/)
2. **Backend**: Fastify + SQLite (server/)

## Setup Instructions

### Backend Setup

1. Navigate to server directory:
```bash
cd server
```

2. Install dependencies:
```bash
npm install
```

3. Create `.env` file:
```bash
cp .env.example .env
```

4. Initialize the database:
```bash
npm run init-db
```

5. Start the development server:
```bash
npm run dev
```

The backend will be available at `http://localhost:3001`

**Demo Credentials:**
- Username: `demo`
- Password: `demo`

### Frontend Setup

1. In the root directory, install dependencies:
```bash
npm install
```

2. Update API endpoint in frontend (if needed):
   - Open `src/main.js`
   - Update API base URL to match backend

3. Start the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173` (or as shown in terminal)

## API Endpoints

All API endpoints are prefixed with `/api`:
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `GET /api/profile` - Get user profile
- `PUT /api/profile` - Update user profile
- `POST /api/change-password` - Change password
- `GET/POST/PUT/DELETE /api/invoices` - Invoice management
- `GET/POST/PUT/DELETE /api/projects` - Project management
- `GET/POST/PUT/DELETE /api/tasks` - Task management
- `GET/POST/PUT/DELETE /api/meetings` - Meeting management

See `server/README.md` for detailed API documentation.

## Environment Variables

### Backend (.env in server/)
```
PORT=3001
NODE_ENV=development
JWT_SECRET=your-secret-key-change-this-in-production
DATABASE_PATH=./src/database/app.db
```

## Running Both Servers

In two separate terminals:

Terminal 1 (Backend):
```bash
cd server
npm run dev
```

Terminal 2 (Frontend):
```bash
npm run dev
```

## Database

The backend uses SQLite with automatic schema creation. The database file is located at `server/src/database/app.db`.

To reset the database, delete `app.db` and run:
```bash
npm run init-db
```

## Building for Production

### Frontend:
```bash
npm run build
npm run preview
```

### Backend:
```bash
cd server
npm start
```

## Project Structure

```
amoth-consulting-services/
├── src/                    # Frontend Vue components
│   ├── components/         # Vue components
│   ├── assets/             # Styles and assets
│   └── main.js             # Vue app entry point
├── server/                 # Backend Fastify API
│   ├── src/
│   │   ├── database/       # Database setup
│   │   ├── middleware/     # Express middleware
│   │   ├── routes/         # API routes
│   │   ├── utils/          # Utilities
│   │   └── server.js       # Main server file
│   └── package.json
├── public/                 # Public assets
├── index.html              # HTML entry point
├── vite.config.js          # Vite configuration
└── package.json            # Frontend dependencies
```

## Troubleshooting

### Port Already in Use
- Backend default: 3001
- Frontend default: 5173
- Change ports in `.env` (backend) or `vite.config.js` (frontend)

### Database Issues
```bash
# Reset database
cd server
rm src/database/app.db
npm run init-db
```

### CORS Errors
- Backend has CORS enabled by default
- If issues persist, update `server/src/server.js`

## Support

For more information:
- Backend API docs: `server/README.md`
- Frontend: `README.md`
