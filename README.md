# LookIT

**LookIT** is a web application similar to a social network (Instagram-style), built with ASP.NET Core. It lets users share posts (text, images, videos), interact via likes and comments, follow other users, create collections, and communicate in groups.

The project was developed **as a team** with **2 other people**, for educational purposes and to practice modern web technologies.

---

## Main features

- **Authentication and accounts** – Registration, login, and profile management (ASP.NET Core Identity)
- **User profile** – Profile picture, bio, public/private profile setting
- **Posts** – Create posts with text, image, or video; edit and delete
- **Likes and comments** – Interact with posts via likes and comments (comments can be edited)
- **Following users** – Follow requests for private profiles; followers/following lists
- **Collections** – Create personal collections and save posts into collections
- **Groups** – Create groups with a moderator, members, and in-group messaging
- **Search** – Search users and content
- **Administration** – Admin panel for users and content
- **Media content analysis** – Analysis of media content from posts and comments, powered by **OpenAI** via an API
- **Security** – Content sanitization (HtmlSanitizer), validation, and database delete policies

---

## Technologies used

| Category     | Technology |
|-------------|------------|
| Backend     | **ASP.NET Core 9.0** (MVC) |
| Auth        | **ASP.NET Core Identity** |
| Database    | **Entity Framework Core 9** – SQLite / SQL Server |
| Frontend    | Razor Views, CSS, JavaScript |
| Packages    | HtmlSanitizer, EF Core Design & Tools |

---

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 (or VS Code / Rider) – optional
- SQL Server or SQLite (default in development)

---

## Installation and running

1. **Clone the repository**
   ```bash
   git clone https://github.com/<username>/LookIT.git
   cd LookIT
   ```

2. **Restore dependencies and run the application**
   ```bash
   dotnet restore
   dotnet run --project LookIT
   ```

3. **Open in browser**
   - The app runs at the URL shown in the console (e.g. `https://localhost:7xxx` or `http://localhost:5xxx`).

4. **Database**
   - On first run, EF Core migrations may be applied if configured. Check `appsettings.json` for the connection string (SQLite or SQL Server).

---

## Project structure (overview)

```
LookIT/
├── LookIT.sln
└── LookIT/
    ├── Areas/Identity/        # Identity pages (login, register, etc.)
    ├── Controllers/           # Profile, Posts, Comments, Likes, Groups, Messages, Collections, Search, Admin
    ├── Data/                  # ApplicationDbContext, migrations
    ├── Models/                # ApplicationUser, Post, Comment, Like, Group, Message, Collection, FollowRequest, etc.
    ├── Services/              # Services (e.g. moderation results, content analysis)
    ├── Views/                 # Razor views for all features
    └── wwwroot/               # Static files (images, scripts, styles)
```

---

## Team

**LookIT** was developed by a **team of 3 people**, with tasks split across modules (authentication, posts, groups, collections, administration, etc.) and collaboration on the same codebase.
