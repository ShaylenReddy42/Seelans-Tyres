cmake_minimum_required(VERSION 3.21.4)

include(FindGit)

project(
	SeelansTyres
	LANGUAGES NONE
)

# Extracting version

set(TAG "0.0.0")
set(REVISION "0")
set(COMMIT_ID "unknown")
set(COMMIT_URL "https://github.com/ShaylenReddy42/Seelans-Tyres")

if(${GIT_FOUND} AND EXISTS "${CMAKE_SOURCE_DIR}/.git")
	execute_process(
		COMMAND ${GIT_EXECUTABLE} rev-list --all --abbrev-commit --abbrev=7 --max-count=1
		WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
		OUTPUT_VARIABLE COMMIT_ID
		OUTPUT_STRIP_TRAILING_WHITESPACE
		ERROR_QUIET
	)
	
	execute_process(
		COMMAND ${GIT_EXECUTABLE} describe --tags --abbrev=0
		WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
		OUTPUT_VARIABLE TAG
		OUTPUT_STRIP_TRAILING_WHITESPACE
		ERROR_QUIET
	)

	if(TAG STREQUAL "")
		set(TAG "0.0.0")
		
		execute_process(
			COMMAND ${GIT_EXECUTABLE} rev-list ${COMMIT_ID} --count
			WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
			OUTPUT_VARIABLE REVISION
			OUTPUT_STRIP_TRAILING_WHITESPACE
			ERROR_QUIET
		)
	else()
		execute_process(
			COMMAND ${GIT_EXECUTABLE} describe --tags
			WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
			OUTPUT_VARIABLE FULL_TAG
			OUTPUT_STRIP_TRAILING_WHITESPACE
			ERROR_QUIET
		)
		
		if(FULL_TAG STREQUAL TAG)
			set(REVISION 0)
		else()
			string(REGEX MATCH "-[0-9]+-" REVISION "${FULL_TAG}")
			string(REGEX REPLACE "-" "" REVISION "${REVISION}")
		endif()
	endif()
endif()

set(FILE_VERSION "${TAG}.${REVISION}")
set(INFORMATIONAL_VERSION "${TAG}+${REVISION}-${COMMIT_ID}")

if(NOT COMMIT_ID STREQUAL "unknown")
	string(APPEND COMMIT_URL "/commit/${COMMIT_ID}")
endif()

message(STATUS "File Version: ${FILE_VERSION}")
message(STATUS "Informational Version: ${INFORMATIONAL_VERSION}")
message(STATUS "Commit Url: ${COMMIT_URL}")

set(
	PROJECTS
	"SeelansTyres.Frontends.Mvc"
	"SeelansTyres.Gateways.MvcBff"
	"SeelansTyres.Libraries.Shared"
	"SeelansTyres.Services.AddressService"
	"SeelansTyres.Services.IdentityService"
	"SeelansTyres.Services.OrderService"
	"SeelansTyres.Services.TyresService"
)

set(COPY_TEMPLATE "")
set(RUNALL_TEMPLATE "")

foreach(PROJECT IN LISTS PROJECTS)
	configure_file(
		"${CMAKE_SOURCE_DIR}/cmake/run.cmd.in"
		"${CMAKE_SOURCE_DIR}/scripts/run/${PROJECT}.cmd")

	string(REPLACE "." ";" TEMP_LIST ${PROJECT})

	list(GET TEMP_LIST 1 SOLUTION_FOLDER)

	configure_file(
		"${CMAKE_SOURCE_DIR}/cmake/${PROJECT}.AssemblyInfo.cs.in" 
		"${CMAKE_SOURCE_DIR}/${SOLUTION_FOLDER}/${PROJECT}/Properties/AssemblyInfo.cs")
	
	if(NOT SOLUTION_FOLDER STREQUAL "Libraries")
		set(COPY_PUBLISH_TEMPLATE "XCOPY /S /Q ${SOLUTION_FOLDER}\\${PROJECT}\\publish\\ publish\\${PROJECT}\\ \nECHO. \n\n")
		set(COPY_RUN_TEMPLATE "COPY /V /Y scripts\\run\\${PROJECT}.cmd publish\\${PROJECT}\\run.cmd \nECHO. \n\n")

		string(APPEND COPY_TEMPLATE ${COPY_PUBLISH_TEMPLATE} ${COPY_RUN_TEMPLATE})

		set(RUN_TEMPLATE "START /min CMD /c ${PROJECT}\\run.cmd \n\n")

		string(APPEND RUNALL_TEMPLATE ${RUN_TEMPLATE})
	endif()
endforeach()

configure_file(
	"${CMAKE_SOURCE_DIR}/cmake/.env.in" 
	"${CMAKE_SOURCE_DIR}/.env")

configure_file(
	"${CMAKE_SOURCE_DIR}/cmake/copy-publish-files.cmd.in" 
	"${CMAKE_SOURCE_DIR}/scripts/copy-publish-files.cmd")

configure_file(
	"${CMAKE_SOURCE_DIR}/cmake/run-all.cmd.in" 
	"${CMAKE_SOURCE_DIR}/scripts/run-all.cmd")

option(CI_BUILD "Used to write the version info to a file for CI" OFF)

if(CI_BUILD)
	file(WRITE "${CMAKE_SOURCE_DIR}/version.txt" ${INFORMATIONAL_VERSION})
endif()