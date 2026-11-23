# 🎮 Obysidian DS4 — Launcher & Gerenciador de Controle de Baixa Latência

> Launcher moderno e otimizador de controle para jogos competitivos desenvolvido em **C# (.NET 8) com WPF**.  
> Projetado para emulação de baixa latência (DS4Windows / DualShock 4 / DualSense / Xbox Controller) com suporte a drivers ViGEmBus e FakerInput.

---

## 🎯 Recursos Principais

- **⚡ Inicialização de Baixa Latência:** Redução no tempo de polling rate do controle via USB/Bluetooth para taxas de 1000Hz (1ms de atraso).
- **🕹️ Mapeamento e Perfis Rápidos:** Integração direta com curvas de resposta (`BezierCurveEditor`) e gatilhos rápidos (Hair Triggers).
- **🛡️ Sistema de Licenciamento Seguro:** Validação de HWID do dispositivo para proteção do software.
- **🎨 Interface Moderna em WPF:** Dark mode elegante, fluido e sem consumo de recursos em segundo plano.

---

## 🛠️ Tecnologias Utilizadas

- **Plataforma:** .NET 8 / .NET 9
- **Interface:** C# / WPF (Windows Presentation Foundation)
- **Drivers de Baixo Nível Suportados:** ViGEmBus (`Nefarius.ViGEm.Client`), FakerInput
- **Ícone e Notificações:** `H.NotifyIcon.Wpf`

---

## 🚀 Como Compilar e Rodar

Abra o arquivo `ObysidianDS4.csproj` no **Visual Studio 2022+** ou compile via CLI:

```bash
dotnet restore
dotnet build --configuration Release
```
