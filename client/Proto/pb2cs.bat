
@echo off

set PROTOC_BIN="..\tools\ProtoGen\protogen.exe"

:: net protocol 
set NET_SRC=".\Net"
set NET_DST="..\Assets\Proto\Net"

:: game table protocol
set TABLE_SRC=".\Table"
set TABLE_DST="..\Assets\Proto\Table"

:: Does NET_DST and TABLE_DST directory exists

:: delete old *.cs files
del /f/s %NET_DST%\*.cs
del /f/s %TABLE_DST%\*.cs

call:pb2Cs_func %NET_SRC%, %NET_DST%
call:pb2Cs_func %TABLE_SRC%, %TABLE_DST%

goto:eof

::@brief convert *.proto to *.cs
:pb2Cs_func
	set "src=%~1"
	set "dst=%~2"

	SETLOCAL ENABLEDELAYEDEXPANSION
	set dir_list='dir /b %src%\*.proto'

	:: TODO: if empty list return

	for /f "delims=" %%v in (%dir_list%)  do (
		set name=%%v

		set iname=!name!
        set INPUT_CS_FILE=!src!/!iname!
        set INPUT_CS_FILE=!INPUT_CS_FILE:"=!

        set oname=!name:proto=cs!
        set OUTPUT_CS_FILE=!dst!/!oname!
        set OUTPUT_CS_FILE=!OUTPUT_CS_FILE:"=!

        %PROTOC_BIN% -i:!INPUT_CS_FILE! -o:!OUTPUT_CS_FILE!
	)
	ENDLOCAL
goto:eof
