--[[
    TODO: 在C层实现一套日志接口
    1. 以避免debug接口的效率问题
    2. 减少字符串GC
]]


logType = 
{
    info = 1,
    warm = 2,
    err = 3,
}

local logType2Name = 
{
    [logType.info] = "info",
    [logType.warm] = "warm",
    [logType.err] = "err",
}

function trace_str(lvl)
    local lvl = lvl + 2
    local s = ""
    local j = 1

    while true do
        local tInfo = debug.getinfo(lvl, "Sfln")
        if tInfo == nil then
            break
        end

        if tInfo.what == "C" then
            s = string.format("%s\n[C]: ?", s)
        else
            s = string.format("%s\n%s:%d in function '%s'", s, tInfo.short_src, tInfo.currentline, tInfo.name or "?")
        end

        -- 本地变量
        j = 1
        while true do
            local k, v = debug.getlocal(lvl, j)
            if k == nil then
                break
            end

            s = string.format("%s\n\t %s:%s: %s", s, tostring(k), type(v), tostring(v))
            j = j + 1
        end
        
        -- upvalues
        if tInfo.func ~= nil then
            j = 1
            while true do
                local k, v = debug.getupvalue(tInfo.func, j)
                if k == nil then
                    break
                end

                s = string.format('%s\n\t %s(upvalue):%s: %s', s, tostring(k), type(v), tostring(v))
                j = j + 1
            end
        end

        lvl = lvl + 1
    end

    return s
end

function trace(v)
    local s = trace_str(1)
    info("%s\n%s", v, s)
end

local function time()
    -- 临时实现， 需要在C层获取时间，精确到毫秒，
    return os.date("%Y-%m-%d,%H:%M:%S")
end

local function log(t, fmt, ...)
    local msg = fmt

    local tmp = ...
    if tmp ~= nil then
        msg = string.format(fmt, ...)
    end

    local log_type_str = tostring(logType2Name[t])
    local time_str = time()
    msg = string.format("%s %s %s", log_type_str, time_str, msg)
    print(msg)
end

function info(fmt, ...)
    log(logType.info, fmt, ...)
end

function err(fmt, ...)
    log(logType.err, fmt, ...)
end

function warm(fmt, ...)
    log(logType.warm, fmt, ...)
end

function print_t(tbl, depth)
    local s = tbl2str(tbl, depth)
    info(s)
end

local _spaceMap = {}
local function getSpace(tabCount)
    if _spaceMap[tabCount] == nil then
        _spaceMap[tabCount] = string.rep(' ', 4 * tabCount)
    end
    return _spaceMap[tabCount]
end

function tbl2str(tbl, depth, cur)
    if type(tbl) ~= "table" then
        info(tostring(tbl))
        return
    end

    depth = depth or 2
    cur = cur or 0

    if cur > depth then
        return "..."
    end

    local s = ""
    for k, v in pairs(tbl) do
        local sub_space_str = getSpace(cur + 1)

        local k_str = ""
        if (type(k) == 'number') then
            k_str = '[' .. k .. ']'
        elseif (type(k) == 'string') then
            k_str = k
        end

        local v_str = ""
        if (type(v) == 'number') then
            local _, f = math.modf(v)
            if f ~= 0 then
                v_str = string.format("%f", v)
            else
                v_str = string.format("%.f", v)
            end
        elseif (type(v) == 'string') then
            v_str = '"' .. v .. '"'
        elseif type(v) == 'table' and cur <= depth then
            v_str = tbl2str(v, depth, cur + 1)
        else
            v_str = tostring(v)
        end

        s = string.format("%s%s=%s", sub_space_str, k_str, v_str)
    end

    local parent_space_str = getSpace(cur)
    return string.format("\n%s{\n%s\n%s}", parent_space_str, s , parent_space_str)
end


-- 控制日志输出级别
function setLogLevel()
    
end


info('hello wolrd')
info('hello world %s', 'lua')
trace('hello world %s', 'lua')

local tbl = {
    [1] = 1.000000000,
    k = 'val',
    t = {1, 2},
    k2 = {
        k3 = {
            k4 = {3, 4, 5}
        }
    }
}
print_t(tbl)


