# Flexlib

![Windows](https://img.shields.io/badge/OS-Windows-blue?logo=windows)

> **A lightweight system for building flexible and interconnected libraries with just a few keystrokes.**

---

<p align="center">
  <img width="100%" src="https://github.com/user-attachments/assets/02ca656e-ee22-4f49-9c62-01b91bbd9858" alt="Flexlib GUI Design" />
</p>
<p align="center"><em>Figure 1:</em> Flexlib GUI design (work in progress)</p>

---

## 📚 What is Flexlib?

Flexlib was born out of the frustration of trying to organize a digital library using traditional file systems. While folders and filenames offer basic structure, they become cumbersome and limiting when:

- Classifying items using multiple properties (e.g. author, theme, rating)
- Reorganizing your collection structure
- Searching or filtering by attributes
- Creating content-based associations between files

Flexlib introduces a **structured yet flexible model** and an **intuitive, minimalistic syntax** to manage physical or digital collections — whether books, PDFs, videos, or any kind of media.

Instead of relying on rigid directory hierarchies, Flexlib treats your library as a **queryable, shapeable dataset**, allowing you to design powerful layouts based on properties.

---

## 📑 Table of Contents

- [Features](#-features)
- [Getting Started](#-getting-started)
- [Usage](#-usage)
- [Screenshots](#-screenshots)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

| Feature                             | Status        |
|-------------------------------------|---------------|
| Property-based classification       | ✅ Implemented |
| Dynamic layout restructuring        | ✅ Implemented |
| Content-based associations          | ✅ Implemented |
| Lightweight CLI                     | ✅ Implemented |
| GUI interface                       | 🚧 In Progress |
| Cross-platform support (Linux/macOS)| 🚧 Planned     |

---

## 🚀 Getting Started

### ✅ Prerequisites

Make sure the following tools are installed:

- [.NET 6.0 SDK or higher](https://dotnet.microsoft.com/en-us/download)
- Windows PowerShell 5.1+ or [Windows Terminal](https://aka.ms/terminal)
- [Git](https://git-scm.com/downloads) – for cloning the repository

---

## 🛠️ Usage

### 🔹 Option 1 — Use Prebuilt Release

1. Go to the [Releases](https://github.com/your-username/flexlib/releases) page.
2. Download the latest `.zip` or `.exe`.
3. Extract the contents into any folder.
4. (Optional) Add that folder to your system `PATH`.
5. Run Flexlib:

    ```powershell
    flexlib --help
    ```

---

### 🔸 Option 2 — Build from Source

```powershell
git clone https://github.com/<your-username>/flexlib.git
cd ./flexlib/
./_commands/build
./_commands/flexlib help
```

---

## 🖼️ Screenshots

> 

---

## 🧭 Roadmap

Here's what’s planned for future versions:

- [x] CLI with flexible layout system
- [x] Library metadata model with multiple custom properties
- [ ] Basic GUI for browsing and editing libraries
- [ ] Saved views
- [ ] Cross-platform support

---

## 🤝 Contributing

We welcome contributions! If you'd like to help:

1. Fork the repository
2. Create a feature branch:
    ```bash
    git checkout -b feature/your-feature
    ```
3. Commit your changes
4. Open a pull request

Also feel free to report issues or suggest ideas!

> Before contributing, make sure to review the `CONTRIBUTING.md` (coming soon).

---

## 📄 License

This project is licensed under the **MIT License**.  
See the [LICENSE](LICENSE) file for full details.
