// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the PERFLIB_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// PERFLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef PERFLIB_EXPORTS
#define PERFLIB_API __declspec(dllexport)
#else
#define PERFLIB_API __declspec(dllimport)
#endif

/////////////////////////////////////////////////////////////////////////////
#define MAX_CPU									8
/////////////////////////////////////////////////////////////////////////////
// define constants / structures and function prototype for NTDLL.dll
// NtQuerySystemInformation function which will be used for CPU usage 
// calculation
/////////////////////////////////////////////////////////////////////////////
#define SystemProcessorPerformanceInformation	8
/////////////////////////////////////////////////////////////////////////////
typedef HRESULT(WINAPI *NTQUERYSYSTEMINFORMATION)(UINT, PVOID, ULONG, PULONG);
/////////////////////////////////////////////////////////////////////////////
typedef struct SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION
{
	LARGE_INTEGER	IdleTime;
	LARGE_INTEGER	KernelTime;
	LARGE_INTEGER	UserTime;
	LARGE_INTEGER	Reserved1[2];
	ULONG			Reserved2;
} SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION;

// This class is exported from the PerfLib.dll
class PERFLIB_API CPerfLib {
public:
	CPerfLib(void);
	// TODO: add your methods here.
};

extern PERFLIB_API int nPerfLib;

PERFLIB_API int fnPerfLib(void);
