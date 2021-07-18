math.randomseed(os.time())

RandUtils = RandUtils or {}

--- 有权重列表{10, 20, 30, 40}, 输出下标{2,3}
function RandUtils.RandByWeight(list, n)
end

--- 有权重列表{[33]=10, [45]=20, [24]=30, [36]=40}, 输出下标{[33]=1,[45]=1}
function RandUtils.RandByWeight2(list, n)
end

function RandUtils.Rand(m, n)
    return math.random(m, n)
end

--- Fisher–Yates shuffle 洗牌算法
function RandUtils.Shuffle(ary)
    local sz = #ary
    if sz <= 0 then
        return
    end

    for i = sz, 2, -1 do
        local r = RandUtils.Rand(1, i - 1)
        local tmp = ary[i]
        ary[i] = ary[r]
        ary[r] = tmp
    end
end


-- for i=1, 99, 1 do
local ary = {1,2,3,4,5}
RandUtils.Shuffle(ary)
local s = ""
for _, v in pairs(ary) do
    s = s .. "," .. v
end

    print(s)
-- end
