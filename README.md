## Comandos

```dotnet add package Microsoft.Diagnostics.Tracing.TraceEvent``` - Instala o pacote NuGet necessário para usar ETW (WIP)

```dotnet format``` - Aplica a formatação de código especificada em .editorconfig

```dotnet run --project MonitorGpu``` - Roda o projeto MonitorGpu da raiz


## Utils (WIP)

RamReader
- [Estrutura usada por GlobalMemoryStatusEx](https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex)

CpuReader
- [Estrutura usada por GetSystemTimes](https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-filetime)

ETW
- [Event tracing](https://learn.microsoft.com/pt-br/windows/win32/etw/event-tracing-portal)
- [Perfview](https://github.com/Microsoft/perfview/blob/main/documentation/TraceEvent/TraceEventLibrary.md)
