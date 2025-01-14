# Trying to perform the tests in the foreach loop unfortunately fails...
# CMake appears to run the check only once, for the first entry in the list,
# probably caching the result using the <var> name and so further tests aren't
# performed.
macro(c_compiler_has_flag _flag)
  string(REGEX REPLACE "-|,|=" "_" flag_name ${_flag})
  check_c_compiler_flag(-${_flag} HAS_${flag_name}_C)
  if (HAS_${flag_name}_C)
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -${_flag}")
  endif()
endmacro(c_compiler_has_flag)

macro(cxx_compiler_has_flag _flag)
  string(REGEX REPLACE "-|,|=" "_" flag_name ${_flag})
  check_cxx_compiler_flag(-${_flag} HAS_${flag_name}_CXX)
  if (HAS_${flag_name}_CXX)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -${_flag}")
  endif()
endmacro(cxx_compiler_has_flag)

macro(linker_has_flag _flag)
  string(REGEX REPLACE "-|,|=" "_" flag_name ${_flag})
  set(CMAKE_REQUIRED_FLAGS "-${_flag}")
  check_c_compiler_flag("" HAS_${flag_name}_LINKER)
  if(HAS_${flag_name}_LINKER)
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -${_flag}")
  endif()
endmacro()

macro(xa_common_prepare)
  if(NOT DSO_SYMBOL_VISIBILITY)
    set(DSO_SYMBOL_VISIBILITY "hidden")
  endif()

  #
  # Currently not supported by NDK clang, but worth considering when it is eventually supported:
  #
  #  -fsanitize=safe-stack
  #

  # Don't put the leading '-' in options
  set(XA_COMPILER_FLAGS
    fno-strict-aliasing
    ffunction-sections
    funswitch-loops
    finline-limit=300
    fvisibility=${DSO_SYMBOL_VISIBILITY}
    fstack-protector-strong
    fstrict-return
    Wa,--noexecstack
    fPIC
    )

  # Using flto seems to breaks LLDB debugging as debug symbols are not properly included
  # thus disable on desktop builds where we care less about its benefits and would rather
  # keep debuggability
  if(NOT MINGW AND NOT WIN32 AND NOT APPLE)
    # -flto leaves a lot of temporary files with mingw builds, turn the optimization off as we don't really need it there
    set(XA_COMPILER_FLAGS ${XA_COMPILER_FLAGS} flto)
  endif()

  if(CMAKE_BUILD_TYPE STREQUAL Debug)
    set(XA_COMPILER_FLAGS ${XA_COMPILER_FLAGS} ggdb3 fno-omit-frame-pointer O0)
  else()
    set(XA_COMPILER_FLAGS ${XA_COMPILER_FLAGS} g fomit-frame-pointer O2)
    add_definitions("-DRELEASE")
  endif()

  set(XA_LINKER_ARGS
    Wl,-z,now
    Wl,-z,relro
    Wl,-z,noexecstack
    Wl,--no-undefined
    )

  if(MINGW)
    set(XA_LINKER_ARGS
      ${XA_LINKER_ARGS}
      Wl,--export-all-symbols
      )
  else()
    set(XA_LINKER_ARGS
      ${XA_LINKER_ARGS}
      Wl,--export-dynamic
      )
  endif()
endmacro()
