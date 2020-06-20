local FUNC_ACT = {}
function FUNC_ACT.New()
    local obj = {}
    obj.mFunc = nil
    obj.mParams = nil
    setmetatable(obj, {__index = FUNC_ACT})
    return obj
end

function FUNC_ACT:Dispose()
    self.mFunc = nil
    self.mParams = nil
end

function FUNC_ACT:Run(_next)
    if self.mFunc ~= nil then
        self.mFunc(_next, unpack(self.mParams))
    end
end

local Sequence = {}

function Sequence.New()
    local obj = {}
    obj.mActList = {}
    setmetatable(obj, {__index = Sequence})
    return obj
end

function Sequence:Dispose()
    for _, act in pairs(self.mActList) do
        if act ~= nil then
            act:Dispose()
        end
    end

    self.list = nil
end

function Sequence:GetFuncObj()
    -- todo: 复用对象
    return FUNC_ACT.New()
end

function Sequence:Append(act)
    table.insert(self.mActList, act)
end

function Sequence:AppendFunc(func, ...)
    local obj = self:GetFuncObj()
    obj.func = func
    obj.params = {...}
    self:Append(obj)
end

function Sequence:Run()
    local i = 0
    local _next = nil
    _next = function()
        i = i + 1
        local act = self.mActList[i]
        if act ~= nil then
            act:Run(_next)
        end
    end
    _next()
end

-- test
local act1 = {}
function act1:Run(_next)
    print("act1 Run")
--创建一个协程,但还没有调用
    local co = coroutine.create(function ()
        os.execute("sleep 2")
        _next()
    end)
    --开启协程
    coroutine.resume(co,1)
end

function act1:Dispose()
    print("act1 Dispose")
end

local act2 = {}
function act2:Run(_next)
    print("act2 Run")
    _next()
end

function act2:Dispose()
    print("act2 Dispose")
end

local seq = Sequence.New()
seq:Append(act1)
seq:AppendFunc(function (_next, v1, v2)
    print("v1 " .. tostring(v1))    
    print("v2 " .. tostring(v2))    
end, 12345, "hello")
seq:Append(act2)
seq:Run()
seq:Dispose()
