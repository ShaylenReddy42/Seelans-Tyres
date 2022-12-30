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

# Key: Solution Folder | Solution Subfolder | Project Name | Is Runnable ?

set(
	PROJECTS
	"Frontends | HealthChecksUISolution | SeelansTyres.Frontends.HealthChecksUI | Yes"
	"Frontends | MvcSolution | SeelansTyres.Frontends.Mvc | Yes"

	"Gateways | MvcBffSolution | SeelansTyres.Gateways.MvcBff | Yes"

	"Libraries | SharedLibrarySolution | SeelansTyres.Libraries.Shared | No"

	"Services | AddressServiceSolution | SeelansTyres.Data.AddressData | No"
	"Services | AddressServiceSolution | SeelansTyres.Models.AddressModels | No"
	"Services | AddressServiceSolution | SeelansTyres.Services.AddressService | Yes"
	"Services | AddressServiceSolution | SeelansTyres.Workers.AddressWorker | Yes"

	"Services | IdentityServiceSolution | SeelansTyres.Models.IdentityModels | No"
	"Services | IdentityServiceSolution | SeelansTyres.Services.IdentityService | Yes"

	"Services | OrderServiceSolution | SeelansTyres.Data.OrderData | No"
	"Services | OrderServiceSolution | SeelansTyres.Models.OrderModels | No"
	"Services | OrderServiceSolution | SeelansTyres.Services.OrderService | Yes"
	"Services | OrderServiceSolution | SeelansTyres.Workers.OrderWorker | Yes"

	"Services | TyresServiceSolution | SeelansTyres.Models.TyresModels | No"
	"Services | TyresServiceSolution | SeelansTyres.Services.TyresService | Yes"
)

set(PUBLISHALL "@ECHO off \n\n")
set(COPY_TEMPLATE "")
set(RUNALL_TEMPLATE "")

foreach(PROJECT IN LISTS PROJECTS)
	string(REPLACE " | " ";" TEMP_LIST ${PROJECT})

	list(GET TEMP_LIST 0 SOLUTION_FOLDER)
	list(GET TEMP_LIST 1 SOLUTION_SUBFOLDER)
	list(GET TEMP_LIST 2 PROJECT_NAME)
	list(GET TEMP_LIST 3 IS_RUNNABLE)

	configure_file(
		"${CMAKE_SOURCE_DIR}/cmake/AssemblyInfo/${PROJECT_NAME}.in" 
		"${CMAKE_SOURCE_DIR}/src/${SOLUTION_FOLDER}/${SOLUTION_SUBFOLDER}/${PROJECT_NAME}/Properties/AssemblyInfo.cs")
	
	if(IS_RUNNABLE STREQUAL "Yes")
		configure_file(
			"${CMAKE_SOURCE_DIR}/cmake/run.cmd.in"
			"${CMAKE_SOURCE_DIR}/scripts/run/${PROJECT_NAME}.cmd")

		file(READ "${CMAKE_SOURCE_DIR}/cmake/publish-project.in" PUBLISH_PROJECT)
		string(REPLACE "@SOLUTION_FOLDER@" "${SOLUTION_FOLDER}" PUBLISH_PROJECT ${PUBLISH_PROJECT})
		string(REPLACE "@SOLUTION_SUBFOLDER@" "${SOLUTION_SUBFOLDER}" PUBLISH_PROJECT ${PUBLISH_PROJECT})
		string(REPLACE "@PROJECT_NAME@" "${PROJECT_NAME}" PUBLISH_PROJECT ${PUBLISH_PROJECT})
		string(APPEND PUBLISHALL "${PUBLISH_PROJECT}")

		set(COPY_PUBLISH_TEMPLATE "XCOPY /S /Q src\\${SOLUTION_FOLDER}\\${SOLUTION_SUBFOLDER}\\${PROJECT_NAME}\\publish\\ publish\\${PROJECT_NAME}\\ \nECHO. \n\n")
		set(COPY_RUN_TEMPLATE "COPY /V /Y scripts\\run\\${PROJECT_NAME}.cmd publish\\${PROJECT_NAME}\\run.cmd \nECHO. \n\n")

		string(APPEND COPY_TEMPLATE ${COPY_PUBLISH_TEMPLATE} ${COPY_RUN_TEMPLATE})

		set(RUN_TEMPLATE "START /min CMD /c ${PROJECT_NAME}\\run.cmd \n\n")

		string(APPEND RUNALL_TEMPLATE ${RUN_TEMPLATE})

		configure_file(
			"${CMAKE_SOURCE_DIR}/orchestration/kubernetes/04-deployments/cmake/${PROJECT_NAME}.yaml"
			"${CMAKE_SOURCE_DIR}/orchestration/kubernetes/04-deployments/${PROJECT_NAME}-configured.yaml")
	endif()
endforeach()

configure_file(
	"${CMAKE_SOURCE_DIR}/cmake/.env.in" 
	"${CMAKE_SOURCE_DIR}/orchestration/docker-compose/.env")

file(WRITE "${CMAKE_SOURCE_DIR}/scripts/publish-all.cmd" ${PUBLISHALL})

configure_file(
	"${CMAKE_SOURCE_DIR}/cmake/copy-publish-files.cmd.in" 
	"${CMAKE_SOURCE_DIR}/scripts/copy-publish-files.cmd")

configure_file(
	"${CMAKE_SOURCE_DIR}/cmake/run-all.cmd.in" 
	"${CMAKE_SOURCE_DIR}/scripts/run-all.cmd")

# Handle certificates for https

set(
	HOSTS
	"health-local.seelanstyres.com"
	"id-local.seelanstyres.com"
	"www-local.seelanstyres.com"

	"kibana-local.seelanstyres.com"
	"rabbitmq-local.seelanstyres.com"
)

foreach(HOST IN LISTS HOSTS)
	set(
		DESTINATIONS
		"${CMAKE_SOURCE_DIR}/orchestration/custom-images/nginx/custom-configuration/ssl"
		"${CMAKE_SOURCE_DIR}/orchestration/kubernetes/01-secrets"
	)

	foreach(DESTINATION IN LISTS DESTINATIONS)
		file(
			COPY 
				"${CMAKE_SOURCE_DIR}/self-signed-certificates/${HOST}.crt"
				"${CMAKE_SOURCE_DIR}/self-signed-certificates/${HOST}.key"
			DESTINATION
				"${DESTINATION}"
		)
	endforeach()

	file(
		COPY 
			"${CMAKE_SOURCE_DIR}/self-signed-certificates/${HOST}.crt"
		DESTINATION
			"${CMAKE_SOURCE_DIR}/src/self-signed-certificates"
	)
endforeach()

option(CI_BUILD "Used to write the version info to a file for CI" OFF)

if(CI_BUILD)
	file(WRITE "${CMAKE_SOURCE_DIR}/version.txt" ${INFORMATIONAL_VERSION})
endif()