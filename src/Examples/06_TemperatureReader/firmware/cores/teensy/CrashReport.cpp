#include <Arduino.h>
#include <CrashReport.h>

size_t CrashReportClass::printTo(Print& p) const
{
  p.println("CrashReport is not available on Teensy 2.0");
  return 1;
}

void CrashReportClass::clear()
{
}

CrashReportClass::operator bool()
{
	return false;
}

CrashReportClass CrashReport;
