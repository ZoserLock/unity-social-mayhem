using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Zen.Debug
{
    public static class RuntimeAssert
    {
        // Test if the condition is true, if not, throw an exception
        [Conditional("DEBUG")]
        public static void IsTrue(bool test, string message = "")
        {
            AssertInternal(test, message);
        }
        
        
        // Test if the condition is false, if not, throw an exception
        [Conditional("DEBUG")]
        public static void IsFalse(bool test, string message = "")
        {
            AssertInternal(!test, message);
        }
        
        // Test if the object is null, if not, throw an exception
        [Conditional("DEBUG")]
        public static void IsNull(object obj, string message = "")
        {
            AssertInternal(obj == null, message);
        }
        
        // Test if the object is not null, if not, throw an exception
        [Conditional("DEBUG")]
        public static void IsNotNull(object obj, string message = "")
        {
            AssertInternal(obj != null, message);
        }
        
        // Force an assert failure
        [Conditional("DEBUG")]
        public static void Fail(string message = "")
        {
            AssertInternal(false, $"Forced failure: {message}");
        }
        
        private static string GetLocationString()
        {
            var stackFrame = new StackFrame(2, true);
            var fileName = stackFrame.GetFileName();
            var lineNumber = stackFrame.GetFileLineNumber();
            
            return $"{fileName}:{lineNumber}";
        }
        
        private static void AssertInternal(bool condition, string message)
        {
            if (!condition)
            {
                var locationString = GetLocationString();

                if (Debugger.IsAttached)
                {
                    Console.WriteLine($"Assertion failed: [{locationString}] {message}");
                    Debugger.Break(); // This will break the debugger
                }
                else
                {
                    // This will crash the application
                    System.Diagnostics.Debug.Assert(false, $"Assertion failed: [{locationString}] {message}");
                }
            }
        }
    }
}
