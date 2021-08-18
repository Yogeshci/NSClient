/// <summary>
/// Copyright  NetSuite Inc. 1999. All rights reserved.
/// </summary>
 
using System;

namespace NSClient
{
	/// <summary>
	/// Summary description for Logger.
	/// </summary>
	public class Logger
	{
		private const String LEVEL_INFO = "info";
		private const String LEVEL_DEBUG = "debug";

		private String _level;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Logger( String level )
		{
			//
			// TODO: Add constructor logic here
			//

			_level = level;

		}

        
		/// <summary>
		/// Private method that writes a string to the console
		/// </summary>
		private void Log( String msg )
		{
			System.Console.Out.WriteLine( msg );
		}


		/// <summary>
		/// Logs if level is LEVEL_INFO
		/// </summary>
		public void Info( String msg )
		{
			if ( _level == LEVEL_INFO || _level == LEVEL_DEBUG )
			{
				Log( msg );
			}
		}
		/*
		public void Info( String msg, bool newLine )
		{
			if ( _level == LEVEL_INFO || _level == LEVEL_DEBUG )
			{
				if ( newLine )
					log( "\n--- INFO  " + msg );
				else
					log( "--- INFO  " + msg );
			}
		}
		*/
    
        
		/// <summary>
		/// Logs if level is LEVEL_DEBUG
		/// </summary>
		public void Debug( String msg )
		{
			if ( _level == LEVEL_DEBUG )
			{
				Log( "--- DEBUG: " + msg );
				//log( "*** WARNING: " + msg );
				//log( "***   ERROR: " + msg );
			}
		}

        
		/// <summary>
		/// Logs SOAP faults
		/// </summary>
		public void Fault( String fault, String code, String msg )
		{
			Log( "\n***   SOAP FAULT: fault type=" + fault + " with code=" + code + ". " + msg );
		}


		/// <summary>
		/// Logs SOAP faults
		/// </summary>
		public void Fault( String msg )
		{
			Log( "[SOAP Fault]: " + msg );
		}


		/// <summary>
		/// Logs warnings
		/// </summary>
		public void Warning( String msg )
		{
			Log( "*** WARNING: " + msg );
		}

        
		/// <summary>
		/// Logs an error message
		/// </summary>
		public void Error( String msg )
		{
			Log( "[Error]: " + msg );
		}

		/// <summary>
		/// Logs an error message with a new line
		/// </summary>
		public void Error( String msg, bool isNewLine )
		{
			if (isNewLine)
				Log( "\n[Error]: " + msg );
			else
				Log( "[Error]: " + msg );
		}


		public void ErrorForRecord( String msg )
		{
			Log( "    [Error]: " + msg );
		}

		/// <summary>
		/// Writes to the console
		/// </summary>
		public void Write( String text )
		{
			System.Console.Write( text );
		}

		/// <summary>
		/// Writes line to the console
		/// </summary>
		public void WriteLn( String text )
		{
			System.Console.WriteLine( text );
		}

		/// <summary>
		/// Reads line from the console
		/// </summary>
		public String ReadLn()
		{
			return System.Console.ReadLine().Trim();
		}

        /// <summary>
        /// Reads line with a password from the console
        /// </summary>
        public String ReadPassword()
        {
            String password = "";
            Boolean stop = false;
            do
            {
                ConsoleKeyInfo c = System.Console.ReadKey(true);
                if (c.Key == ConsoleKey.Enter)
                {
                    stop = true;
                }
                else if (c.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password.Remove(password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    password += c.KeyChar;
                    Console.Write("*");
                }
            } while (!stop);

            Console.Write("\n");            
            return password.Trim();
        }
    }
}
