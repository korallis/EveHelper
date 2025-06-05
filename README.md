# EVE Helper

A modern WPF ship fitting tool for EVE Online built with .NET 8, featuring ESI API integration and Material Design UI.

## 🚀 Features

- **EVE Online ESI Integration**: Secure OAuth 2.0 authentication with PKCE flow
- **Character Management**: Multi-character support with secure token storage
- **Ship Fitting Tools**: Advanced fitting calculator and optimizer
- **Material Design UI**: Modern, responsive interface with EVE Online theming
- **Real-time Data**: Live market data and ship statistics via ESI API
- **Offline Capability**: Local database for enhanced performance

## 🏗️ Architecture

EVE Helper follows a clean, layered architecture:

- **EveHelper.App**: WPF presentation layer with MVVM pattern
- **EveHelper.Core**: Domain models, interfaces, and business logic
- **EveHelper.Services**: Business services and ESI API integration
- **EveHelper.Data**: Data access layer with LiteDB storage

## 🛠️ Technology Stack

- **.NET 8**: Latest .NET framework
- **WPF**: Windows Presentation Foundation
- **Material Design in XAML**: Modern UI components
- **LiteDB**: Lightweight NoSQL database
- **Microsoft.Extensions.DependencyInjection**: Built-in dependency injection
- **System.IdentityModel.Tokens.Jwt**: JWT token handling
- **ESI API**: EVE Online's official API

## 📋 Prerequisites

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (recommended) or JetBrains Rider
- EVE Online Developer Application (for ESI access)

## 🔧 Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/korallis/EveHelper.git
   cd EveHelper
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure EVE ESI Authentication**:
   - Create an application at [EVE Developers](https://developers.eveonline.com/)
   - Update authentication settings in the app configuration
   - Set callback URL to: `http://localhost:5000/callback`

4. **Build the solution**:
   ```bash
   dotnet build
   ```

5. **Run the application**:
   ```bash
   dotnet run --project EveHelper.App
   ```

## 🔒 EVE Online ESI Setup

To use EVE Helper, you'll need to create an ESI application:

1. Visit [EVE Developers](https://developers.eveonline.com/)
2. Create a new application
3. Set the callback URL to: `http://localhost:5000/callback`
4. Configure the required scopes for character and fitting data
5. Add your Client ID and Secret to the application settings

## 🚧 Development Status

EVE Helper is currently in active development. Current progress:

- ✅ **Task 1**: Project Architecture and Foundation (Complete)
  - Multi-layered architecture with DI
  - Material Design UI theming
  - MVVM pattern with navigation

- 🔄 **Task 2**: EVE Online ESI Authentication (In Progress)
  - ✅ OAuth 2.0 PKCE authentication flow
  - ⏳ Secure token storage
  - ⏳ ESI client service
  - ⏳ Character selection interface
  - ⏳ Character data retrieval

- 📋 **Upcoming Features**:
  - Ship database and fitting tools
  - Market data integration
  - Advanced fitting optimizer
  - Multi-character support

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎮 EVE Online

EVE Helper is a third-party application and is not affiliated with or endorsed by CCP Games. EVE Online and all related characters, names, marks, and logos are trademarks or registered trademarks of CCP hf.

## 📞 Support

If you encounter any issues or have questions, please open an issue on GitHub.

---

**Fly safe, capsuleer! 🚀** 