#include "luajit/lua.hpp";

int main()
{
    lua_State* L = luaL_newstate();
    luaL_openlibs(L);
    luaL_dostring(L, "print('hello world')");
    lua_close(L);
    return 0;
}