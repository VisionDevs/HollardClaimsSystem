
markdown
# Hollard Claims Management System

**Author:** Vision Mavhungu  
**Role:** Junior Software Developer  
**Company:** Hollard Insurance

## What This System Does

A document management system that lets claim verifiers review documents WITHOUT downloading them - saving time and making the claims process faster.

## Key Features

- **View documents inline** - PDFs and images open right in the browser
- **Submit claims** - Easy form with drag & drop upload
- **Verify documents** - Approve, reject, or request revision
- **Add comments** - Explain why a document was rejected
- **No double reviews** - Once reviewed, documents are locked

## Tech Stack

| Technology | What it's used for |
|------------|-------------------|
| C# .NET 8.0 | Backend API |
| ASP.NET Core | Web framework |
| HTML/CSS/JS | Frontend interface |
| File system | Document storage |

## Project Structure
HollardClaimsSystem/
├── Controllers/DocumentsController.cs # API endpoints
├── Models/Document.cs # Data structures
├── Services/DocumentService.cs # Business logic
├── wwwroot/index.html # User interface
└── Program.cs # App setup

text

## How to Run

1. Open in Visual Studio 2022+
2. Press **F5** to run
3. Go to `https://localhost:7159/index.html`

## How to Use

### Submit a Claim
1. Fill in the form
2. Upload documents (PDF, JPG, PNG)
3. Click "Submit Claim for Review"

### Review a Claim
1. Click "Review Claims" tab
2. Click on any document
3. View it right in the browser (no download!)
4. Add comments and approve/reject

## API Endpoints

| Method | Endpoint | What it does |
|--------|----------|--------------|
| POST | `/api/documents/upload` | Upload a file |
| GET | `/api/documents/view/{id}` | View file inline |
| GET | `/api/documents/all` | Get all files |
| POST | `/api/documents/{id}/review` | Submit decision |

## Business Value

- **Saves time** - Verifiers don't download files
- **Better security** - Documents stay in the system
- **Clear audit trail** - All decisions are recorded
- **Faster claims** - Customers get responses quicker

## GitHub Repository

https://github.com/VisionDevs/HollardClaimsSystem

---

**Status:** Complete and ready for demonstration
