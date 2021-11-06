// PerfLib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "PerfLib.h"
#include <shlwapi.h>
#include <float.h>
#include <io.h>

// This is an example of an exported variable
PERFLIB_API int nPerfLib=0;


float						CalcCpuUsage(DWORD dwCpu);

DWORD						m_dwNumberOfProcessors;
NTQUERYSYSTEMINFORMATION	m_pNtQuerySystemInformation;

DWORD						m_dwTickCount[MAX_CPU];
LARGE_INTEGER				m_idleTime[MAX_CPU];
FLOAT						m_fltUsage[MAX_CPU];

// This is an example of an exported function.
PERFLIB_API int fnPerfLib(void)
{
    return 42;
}

PERFLIB_API void init()
{
	SYSTEM_INFO info;
	GetSystemInfo(&info);

	m_dwNumberOfProcessors = info.dwNumberOfProcessors;
	m_pNtQuerySystemInformation = (NTQUERYSYSTEMINFORMATION)GetProcAddress(GetModuleHandleA("NTDLL"), "NtQuerySystemInformation");

	for (DWORD dwCpu = 0; dwCpu<MAX_CPU; dwCpu++)
	{
		m_idleTime[dwCpu].QuadPart = 0;
		m_fltUsage[dwCpu] = FLT_MAX;
		m_dwTickCount[dwCpu] = 0;
	}
}




PERFLIB_API float CalcCpuUsage(DWORD dwCpu)
{
	//validate NtQuerySystemInformation pointer and return FLT_MAX on error

	if (!m_pNtQuerySystemInformation)
		return FLT_MAX;

	//validate specified CPU index and return FLT_MAX on error

	if (dwCpu >= m_dwNumberOfProcessors)
		return FLT_MAX;

	DWORD dwTickCount = GetTickCount();
	//get standard timer tick count

	if (dwTickCount - m_dwTickCount[dwCpu] >= 1000)
		//update usage once per second
	{
		SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION info[MAX_CPU];

		if (SUCCEEDED(m_pNtQuerySystemInformation(SystemProcessorPerformanceInformation, &info, sizeof(info), NULL)))
			//query CPU usage
		{
			if (m_idleTime[dwCpu].QuadPart)
				//ensure that this function was already called at least once
				//and we have the previous idle time value
			{
				m_fltUsage[dwCpu] = 100.0f - 0.01f * (info[dwCpu].IdleTime.QuadPart - m_idleTime[dwCpu].QuadPart) / (dwTickCount - m_dwTickCount[dwCpu]);
				//calculate new CPU usage value by estimating amount of time
				//CPU was in idle during the last second

				//clip calculated CPU usage to [0-100] range to filter calculation non-ideality

				if (m_fltUsage[dwCpu] < 0.0f)
					m_fltUsage[dwCpu] = 0.0f;

				if (m_fltUsage[dwCpu] > 100.0f)
					m_fltUsage[dwCpu] = 100.0f;
			}

			m_idleTime[dwCpu] = info[dwCpu].IdleTime;
			//save new idle time for specified CPU
			m_dwTickCount[dwCpu] = dwTickCount;
			//save new tick count for specified CPU
		}
	}

	return m_fltUsage[dwCpu];
}

// This is the constructor of a class that has been exported.
// see PerfLib.h for the class definition
CPerfLib::CPerfLib()
{
    return;
}
