# 💬 ChatApp – Full Setup & Deployment Guide

A real-time chat application built with **ASP.NET Core 10**, **SignalR**, and **Entity Framework Core**, using **SQL Server** as the backend.

---
## 📂 Project Structure
```scss
src/
├── ChatApp.slnx                  
└── ChatApp/                      
    ├── Controllers/              
    ├── Data/                     
    ├── Hubs/                    
    ├── Models/                  
    ├── Views/                     
    ├── wwwroot/                  
    ├── .env                      
    ├── ChatApp.csproj            
    └── Program.cs                
```
---

# 🗄️ 1. Backend Setup (Microsoft SQL Server)

## 📥 Installation

- Install **SQL Server 2022 Express:**
https://go.microsoft.com/fwlink/p/?linkid=2216019&clcid=0x409&culture=en-us&country=us


- Install **SQL Server Management Studio (SSMS):**
https://aka.ms/ssms/22/release/vs_SSMS.exe

---
## ⚙️ Server Configuration

1. Open **SSMS**
2. Connect to `localhost` using **Windows Authentication**

In the Object Explorer:
- Right-click your **Server Name(localhost)** → **Properties**
- Go to **Security**
- Select:
SQL Server and Windows Authentication mode
- Click **OK**
- Restart SQL Server (Important)

⚠️ SQL logins will NOT work until SQL Server service is restarted.

---

## 🧑‍💻 Create Login
In the Object Explorer:

1. Go to **Security → Logins**
2. Right-click → **New Login**

Fill:

- **Login Name:** `chatapp_admin`
- **Authentication:** SQL Server Authentication
- **Password:** `<your_passwd>`
- Uncheck: `Enforce password policy`
- In the left hand Go to **Server Roles**
- Check: `sysadmin`

Click **OK**

---

## 🗃️ Create Database
In the Object Explorer:

1. Right-click **Databases**
2. Click **New Database**
3. Name it: `ChatAppDB`
Click **OK**

---

# 🛠️ 2. Application Setup (Command Line)

## ✅ Prerequisites

- Install **.NET 10 SDK:**
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-10.0.103-windows-x64-installer

- Install **.NET 10 Hosting Bundle** (Required for IIS):
https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.3/dotnet-hosting-10.0.3-win.exe

---

## 🔐 Environment Configuration

Create a `.env` file inside:
src/ChatApp/

Add the following:

```bash
CHATA_DB_CONN=Server=<DB-SERVER_IP>;Database=ChatAppDB;User Id=chatapp_admin;Password=<Password>;TrustServerCertificate=True;
CHATA_UPLOAD_PATH=D:\ChatAppStorage\
```
⚠️ Ensure the folder D:\ChatAppStorage\ exists.

The IIS Application Pool identity must have write permissions on this folder.
You may use a different path, but make sure:

- The folder exists.
- The same path is configured here.
- Proper permissions are granted.

---

## 🏗️ Build & Apply Migrations

Open terminal in the folder containing `ChatApp.csproj`

```powershell
cd src/ChatApp

# Install EF Tool (once)
dotnet tool install --global dotnet-ef

# Restore dependencies
dotnet restore

# Build project
dotnet build -c Release

# Apply migrations (creates tables)
dotnet ef database update

# to test locally
dotnet run
```

# 🚀 3. IIS Deployment (Production)

### 📦 Publish the Application

```powershell
dotnet publish -c Release -o D:\Publish
```
Note: Copy the .env file as well in D:\Publish folder manually
### Install IIS (Windows Server)

Run PowerShell as Administrator:
```powershell
Install-WindowsFeature -name Web-Server, Web-WebSockets
```
Restart IIS after installation:
```powershell
iisreset
```

### 🌐 Configure IIS
Create Website

- Open IIS Manager
- Right-click Sites → Add Website
Set:
- Physical Path: D:\Publish
- Port: 80 (or custom 5000)

### ⚙️ Application Pool Settings

- Go to Application Pools
- Select your site pool → Right Click
- Click Basic Settings

Set:
- .NET CLR Version → No Managed Code

### Grant Folder Permissions

Run PowerShell as Administrator:

```powershell
icacls "D:\Publish" /grant "IIS AppPool\<YourAppPoolName>:(OI)(CI)RX"
icacls "D:\ChatAppStorage" /grant "IIS AppPool\<YourAppPoolName>:(OI)(CI)F"
```
---

