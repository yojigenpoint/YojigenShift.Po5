# Contributing to Path of the Five Elements: Return to Origin

First off, thank you for considering contributing to this project! It's people like you who make the open-source community such a great place to learn, inspire, and create.

## 📜 Code of Conduct

By participating in this project, you agree to abide by our standards of professional and respectful communication.

## 🛠 Coding Standards

To maintain consistency across the project, please follow these rules:

- **Language**: All code comments, variable names, and documentation must be in **English**.

- **Localization**: Do not hardcode any user-facing strings (especially Chinese characters) in the source code. Use the `LocaleManager` and update the appropriate files in `assets/i18n/`.

- **Framework**: Use `YiFramework` for all Yi-ology related logic. Do not reinvent core algorithms.

- **Godot Patterns**:

  - Use C# 12 features where appropriate.

  - Prefer signals over direct node references for decoupling.

  - Maintain a strict separation between Logic (`.cs` files) and Presentation (`.tscn` files).

## 🐛 Reporting Bugs

Before creating a bug report, please check the existing issues. If you find a new bug, please include:

- A clear, descriptive title.

- Steps to reproduce the bug.

- Expected vs. Actual behavior.

- Screenshots or video if applicable (especially for UI/Physics issues).

## 💡 Feature Requests

We welcome new ideas! Especially those that improve accessibility for **senior users**. Please describe the goal of the feature and how it fits into the "Return to Origin" theme.

## 🚀 Pull Request Process

1. Fork the repository and create your branch from `main`.

2. Ensure your code follows the coding standards mentioned above.

3. Update the documentation/README if you've added new features or changed existing ones.

4. Issue the PR with a clear description of what was changed and why.

## ⚖️ License

By contributing, you agree that your contributions will be licensed under its MIT License.
