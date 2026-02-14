# Path of the Five Elements: Return to Origin

[![Godot Engine](https://img.shields.io/badge/Godot-4.6-%23478cbf?logo=godot-engine&logoColor=white)](https://godotengine.org)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A mobile-first strategy puzzle game designed to bridge the gap between traditional Yi-ology (I Ching) wisdom and modern casual gaming. 

## 🌟 Overview
*Path of the Five Elements: Return to Origin* leverages physics-based stacking mechanics to teach the fundamental relationships of WuXing (Five Elements). The game is specifically optimized for **senior users** and beginners, featuring high-contrast visuals, large touch targets, and a meditative pace.

## 🛠 Tech Stack
- **Engine**: Godot Engine v4.6 (Stable) .NET Edition.
- **Logic Engine**: Powered by [YiFramework](https://www.nuget.org/packages/YojigenShift.YiFramework) (via NuGet).
- **Language**: C# 12 / .NET 9.
- **Platform**: Android & iOS.

## 📂 Project Structure
This project follows a strict separation between Game Logic (C#) and Presentation (Godot Scenes). 
- All code comments are in **English**.
- No hardcoded Chinese strings in the source code; all UI text is handled via the localization system.

## 🚀 Getting Started
1. **Clone the repository**:
   ```bash
   git clone https://github.com/YourUsername/YoshigenShift.Po5.git

2. **Restore NuGet Packages**: The project depends on `YiFramework`. Ensure you have the NuGet CLI or use IDE integration to restore dependencies.

3. **Open in Godot**:
Launch Godot 4.6 (Standard or .NET edition) and import `project.godot`.

📜 Localization

We utilize the localization engine built into `YiFramework`. Translation files (CSV/JSON) can be found in `assets/i18n/`.

🤝 Contributing

Contributions are welcome! Please read our [Contribution Guidelines](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

⚖️ License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
