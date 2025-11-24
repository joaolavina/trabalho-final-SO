# OS Monitor

**OS Monitor** é uma aplicação desktop desenvolvida em C# com .NET 9.0 que monitora em tempo real os recursos do Windows. A aplicação tem interface gráfica (WPF) para visualizar o uso de CPU, GPU, memória RAM e falhas de página (page faults) gerais do sistema e dos processos em execução.

### Monitoramento de Recursos
- **CPU**: Leitura de uso percentual em tempo real através da API `GetSystemTimes`
- **GPU**: Monitoramento de utilização de GPUs usando Windows Management Instrumentation (WMI)
- **Memória RAM**: Rastreamento de uso de memória física com exibição formatada em MB/GB
- **Page Faults**: Monitoramento do sistema e por processo individual

##  Ferramentas
- .NET  9.0 
- WPF (Windows Presentation Foundation)
- C# 
- Visual Studio Code
- .editorconfig

## Dependências
- **Microsoft.Diagnostics.Tracing.TraceEvent** (v3.1.28): Para monitoramento de Event Tracing for Windows (ETW)
- **System.Management** (v9.0.10): Para queries WMI de informações de GPU
- **Microsoft.VisualBasic** (v10.3.0): Utilitários para compatibilidade

### APIs Windows
| Serviço | API | Estrutura | Descrição |
|---------|-----|-----------|-----------|
| CpuReader | GetSystemTimes | FILETIME | Obtém tempos de CPU (kernel, user, idle) |
| RamReader | GlobalMemoryStatusEx | MEMORYSTATUSEX | Obtém status de memória física |
| ProcessInfoReader | GetProcessMemoryInfo | PROCESS_MEMORY_COUNTERS | Informações de memória por processo |
| PageFaultReader | ETW (TraceEvent) | - | Monitora page faults via Event Tracing |

## Comandos
```bash
# Instalar dependências NuGet (se necessário)
dotnet add package Microsoft.Diagnostics.Tracing.TraceEvent

# Executar a aplicação
dotnet run --project OSMonitor

# Formatar código conforme .editorconfig
dotnet format
```

## Referências
- [MEMORYSTATUSEX - GlobalMemoryStatusEx](https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex)
- [FILETIME - GetSystemTimes](https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-filetime)
- [PROCESS_MEMORY_COUNTERS - GetProcessMemoryInfo](https://learn.microsoft.com/en-us/windows/win32/api/psapi/ns-psapi-process_memory_counters)
- [Event Tracing for Windows](https://learn.microsoft.com/pt-br/windows/win32/etw/event-tracing-portal)
- [TraceEvent Documentation - Perfview](https://github.com/Microsoft/perfview/blob/main/documentation/TraceEvent/TraceEventLibrary.md)