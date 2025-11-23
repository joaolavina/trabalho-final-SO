## Comandos

```dotnet add package Microsoft.Diagnostics.Tracing.TraceEvent``` - Instala o pacote NuGet necessário para usar ETW (WIP)

```dotnet format``` - Aplica a formatação de código especificada em .editorconfig

```dotnet run --project MonitorGpu``` - Roda o projeto MonitorGpu da raiz


## Utils

RamReader
- [MEMORYSTATUSEX - Estrutura usada por GlobalMemoryStatusEx](https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex)

CpuReader
- [FILETIME - Estrutura usada por GetSystemTimes](https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-filetime)

ProcessInfoReader
- [PROCESS_MEMORY_COUNTERS - Estrutura usada por GetProcessMemoryInfo](https://learn.microsoft.com/en-us/windows/win32/api/psapi/ns-psapi-process_memory_counters)

ETW
- Event Tracing for Windows (ETW) is a general-purpose, high-speed tracing facility provided by the operating system. Using a buffering and logging mechanism implemented in the kernel, ETW provides a tracing mechanism for events raised by both user-mode applications and kernel-mode device drivers.
- The TraceEvent library is NuGET package that provides a set of .NET Runtime classes that make it easy to control and consume (parse) the strongly typed Event Tracing for Windows (ETW) events.
- [Event tracing](https://learn.microsoft.com/pt-br/windows/win32/etw/event-tracing-portal)
- [Perfview](https://github.com/Microsoft/perfview/blob/main/documentation/TraceEvent/TraceEventLibrary.md)
