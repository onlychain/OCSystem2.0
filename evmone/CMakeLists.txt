cmake_minimum_required (VERSION 3.0)

project ("evmone")

add_compile_definitions (
	PROJECT_VERSION="0.6.0-dev"
)

if (NOT CMAKE_SYSTEM_NAME MATCHES "Windows")
	set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fPIC -fvisibility=hidden")
endif()

include_directories (
	./include
)

set (CMAKE_CXX_STANDARD 17)
aux_source_directory(. DIR_SRCS)
add_library (evmone SHARED ${DIR_SRCS})