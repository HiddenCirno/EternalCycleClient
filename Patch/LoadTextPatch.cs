using HarmonyLib;
using System;
using System.Collections.Generic;

namespace EternalCycleClient.Patch
{
    [HarmonyPatch(typeof(GClass2348), nameof(GClass2348.Localized), new Type[] { typeof(string), typeof(string) })]
    public class LoadTextPatch
    {
        private static readonly Random random = new Random();

        private static readonly Dictionary<string, string> LoadingTextDicrt = new Dictionary<string, string>()
        {
            { "loading_text_001", "What is a wave without the ocean? A beginning without an end?" },
            { "loading_text_002", "They are different, but they go together." },
            { "loading_text_003", "Now you go among the stars, and I fall among the sand." },
            { "loading_text_004", "We are different. But we go....together." },
            { "loading_text_005", "你知道吗？长按「G」键并滑动鼠标滚轮 可以选择需要扔出的投掷物" },
            { "loading_text_006", "5L2g5peg5LiN5peg6IGK77yf" },
            { "loading_text_007", "We got a job to do." },
            { "loading_text_008", "游戏太黑看不清？试试Amand's Graphic模组吧" },
            { "loading_text_009", "如果你的腿骨折或损毁了，而你没有携带止痛效果，那么你将无法奔跑" },
            { "loading_text_010", "还在打开背包治疗自己吗？将鼠标放在胸挂和口袋里的可使用物品上按下数字键以进行快捷键绑定" },
            { "loading_text_011", "一把武器耐久越低，就越容易发生故障，当然，近战武器除外" },
            { "loading_text_012", "通常来讲，一把枪的上限取决于它最终能安装多少配件" },
            { "loading_text_013", "如果你真的几乎花光了所有钱，至少也要装备一个耳机再进行游戏" },
            { "loading_text_014", "作为Scav进行游戏时，你不会被其他Scav主动攻击" },
            { "loading_text_015", "如果你的手臂骨折或损毁了，而你没有携带止痛效果，那么你将无法平稳的举枪瞄准" },
            { "loading_text_016", "骨折可以使用夹板、Grizzly急救包或者手术包进行治疗" },
            { "loading_text_017", "如果你的水分或能量值降至0，你几乎无法自然回复耐力" },
            { "loading_text_018", "在游戏的前期，不携带近战武器可以让你减轻一些重量负担" },
            { "loading_text_019", "在Fence的信任度达到最高等级前，即使你作为Scav进行游戏，也会被Boss攻击" },
            { "loading_text_020", "在负重过高时接敌不是个好选择，也许你应该暂时丢掉背包" },
            { "loading_text_021", "如果你没有足够的把握，最好不要使用近战武器攻击敌人" },
            { "loading_text_022", "作为Scav进入战局和以你的PMC主角色进入战局是不同的，玩家操控的Scav会在随机地点和时间出生，装备也会是随机的" },
            { "loading_text_023", "作为Scav进行游戏你不必担心因为死亡导致主角色装备掉落，如果你在对局中成功幸存，所有装备和搜集的物品可以被转移到你的库存中" },
            { "loading_text_024", "当你扮演Scav时，你的主要角色将不会得到任何经验值或是升级技能" },
            { "loading_text_025", "在战局中双击「O」键可以查看目前可用的撤离点" },
            { "loading_text_026", "如果你在战局中停留太久，直至战局的倒计时归零都还没有撤离，你的角色会被认定为在行动中失踪" },
            { "loading_text_027", "装填弹匣时，记得先检查它们是否兼容" },
            { "loading_text_028", "在战局中时，点击背包页面最上方的健康标签页显示身体各部位的健康状态" },
            { "loading_text_029", "购买物品时，记得勾选左上角的「提供所需物品」按钮来支付费用" },
            { "loading_text_030", "商人有不同的信任度等级，每次升级都可以解锁新商品" },
            { "loading_text_031", "枪械故障时，按下「L」检视枪械确认故障类型，之后按下「Shift + T」排除故障" },
            { "loading_text_032", "除了低耐久，长时间射击导致的过热也会增大枪械故障的可能性" },
            { "loading_text_033", "生命因何而沉睡?" },
            { "loading_text_034", "商人的信任度可以通过完成任务来获取" },
            { "loading_text_035", "在很久之前，护甲机制并没有现在这么复杂" },
            { "loading_text_036", "护甲的部分防弹插板是可以更换的，右键它并选择搜索关联以查找兼容的物品" },
            { "loading_text_037", "右键弹匣并选择搜索关联可以快速找到弹匣对应的弹药，但请注意，你的枪械可能无法发射弹匣所兼容的弹药" },
            { "loading_text_038", "SPT曾经叫做SPT-AKI，在Senko-San离开项目组后，AKI的后缀被删除了，这个项目的前身是JET，而那就是另外一段故事了……" },
            { "loading_text_039", "BSG并不拥有游戏中大部分武器的版权，恰恰相反，他们在游戏中拥有一段免责声明以避免版权问题" },
            { "loading_text_040", "警告，限制区域电力即将重启，门禁将在重启后失效，请所有安保人员保持高度警戒" },
            { "loading_text_041", "Also try Delta Force！" },
            { "loading_text_042", "Also try Arena Breakout Infinity！" },
            { "loading_text_043", "我们还有最后一个办法" },
            { "loading_text_044", "哈夫克与你同频，信息予你无限" },
            { "loading_text_045", "在时间面前，一切问题都是有解的" },
            { "loading_text_046", "厌倦了永不停歇的战斗？也许你可以换一种玩法，比如多捡一些东西" },
            { "loading_text_047", "哀余生之须臾，羡长江之无穷……可是长江之水也总有一天会流尽，何况人短暂的一生呢？" },
            { "loading_text_048", "Welcome aboard, captain, all systems online." },
            { "loading_text_049", "在很久以前，塔科夫市流传着关于一个神秘组织「末日商会」的传说" },
            { "loading_text_050", "逃离塔科夫是一款硬核战术射击游戏，这意味着它永远不可能走向大众" },
            { "loading_text_051", "你知道TENET_吗？他是国内塔科夫圈土皇帝（已驾崩）" },
            { "loading_text_052", "在很久以前，塔科夫市出现过一个叫Lure的神秘姑娘，你可能见过她" },
            { "loading_text_053", "如果你真的很喜欢俄式武器，那么你最好早点卸载逃离塔科夫" },
            { "loading_text_054", "固定武器只有在AI手里才能发挥它应有的威力，作为玩家，你最好别碰那东西" },
            { "loading_text_055", "通常来讲，击杀BOSS能够取得丰厚的战利品奖励，但是话又说回来，有时候BOSS确实几乎无法给你提供任何收益" },
            { "loading_text_056", "本MOD仅在Oddba论坛发布，论坛地址sns.oddba.cn，如果你花钱购买了SPT，我建议你痛骂卖家" },
            { "loading_text_057", "请不要站在BTR的行进路线上挂机——好吧，尽量避免任何在野外的挂机行为，那很危险" },
            { "loading_text_058", "Beware, beware, the daughter of the sea...." },
            { "loading_text_059", "灯塔和街区这两张地图往往会造成极大的资源占用，如果你的电脑配置不够，可能会发生崩溃" },
            { "loading_text_060", "部分撤离点需要特定条件才能撤离，它们往往会在你激活撤离点时出现提示，信号弹撤离点除外" },
            { "loading_text_061", "「收藏家」任务所需的每一件物品几乎都代表一位塔科夫主播" },
            { "loading_text_062", "宇宙再大，也有尽头，而野心永无止境" },
            { "loading_text_063", "如果修剪、施肥、浇水、施药都不管用，你该怎么做？" },
            { "loading_text_064", "想要攀登山峰，总需低头赶路" },
            { "loading_text_065", "海上生明月，天涯共此时" },
            { "loading_text_066", "秩序的强大之处不在于律法之下没有黑暗，而在于秩序永远不会认可这种黑暗" },
            { "loading_text_067", "无论结果如何，努力总好过放弃" },
            { "loading_text_068", "鸟为什么会飞？" },
            { "loading_text_069", "人们纪念的是英雄的形象，而不是英雄的真相" },
            { "loading_text_070", "你的名字无人知晓，你的功绩永世长存" },
            { "loading_text_071", "Son, I want to offer you a second chance." },
            { "loading_text_072", "War, war never changes...." },
            { "loading_text_073", "你有见过那个叫Lotus的姑娘吗？我对她手上的某些技术很感兴趣……" },
            { "loading_text_074", "金色的美梦要开始躁动了……" },
            { "loading_text_075", "站在安稳的时代评论前人铺路时的取舍是一种愚蠢且短视的行为" },
            { "loading_text_076", "文明的遗产不应该变成个人的收藏" },
            { "loading_text_077", "生活总得继续下去——恐慌和埋怨并不会叫醒太阳" },
            { "loading_text_078", "有些人只希望看到他想看的，然后想着法子给你贴标签扣帽子拉仇恨" },
            { "loading_text_079", "无论在哪个世界，总有人秉持着守护的志愿，憧憬即将到来的明天" },
            { "loading_text_080", "民族主义就像底裤，必要时可以露出来给人看，但不能无时无刻都露在外面" },
            { "loading_text_081", "战争是流血的政治，政治是不流血的战争" },
            { "loading_text_082", "花开花落，尽归尘土，缘起缘灭，曲终人散" },
            { "loading_text_083", "想明白生命的意义吗？想要真正的……活着吗？" },
            { "loading_text_084", "人有五名……" },
            { "loading_text_085", "我们活着，就是对恶意最大的反抗" },
            { "loading_text_086", "吾生也有涯，而知也无涯……令人悲伤的事实" },
            { "loading_text_087", "你说的对，但是逃离塔科夫是一款由BSG工作室研发的硬核战术射击游戏……" },
            { "loading_text_088", "在时间的尽头，我们终将重逢" },
            { "loading_text_089", "没有人的文明毫无意义" },
            { "loading_text_090", "憧憬是和理解最遥远的距离" },
            { "loading_text_091", "Also try Escape From Duckov!" },
            { "loading_text_092", "厌倦了塔科夫的混乱的话，就暂时休息一下吧，我会一直注视着你的，预言家……" },
            { "loading_text_093", "我曾目睹星辰焚寂，沉入永夜；我曾见证光年之外，王朝兴灭……" },
            { "loading_text_094", "不破其旧，无以立新！" },
            { "loading_text_095", "我梦见一片焦土，一株破土而生的新蕊，它迎着朝阳绽放，向我低语呢喃……" },
            { "loading_text_096", "人として....生きて下さい" },
            { "loading_text_097", "即使引导早已破碎，还请您当上艾尔登之王" },
            { "loading_text_098", "生存还是毁灭？这是个问题……" },
            { "loading_text_099", "六十二公分，十七公斤重——真沉啊" },
            { "loading_text_100", "那些上岸的鱼再也不是鱼了，同样，真正进入太空的人，再也不是人了" },
            { "loading_text_101", "Hey, you. You finally awake." },
            { "loading_text_102", "对未来的真正慷慨，是把一切都献给现在" }
        };

        [HarmonyPostfix]
        public static void Postfix(string id, string prefix, ref string __result)
        {
            if (id == "Profile data loading...")
            {
                __result = $"<color=#FFFF55>{(LoadingTextDicrt.TryGetValue($"loading_text_{random.Next(1, 103):D3}", out var val) ? val : "奇怪，你怎么会看到这个？你的游戏好像出了点问题……")}</color>";
            }
        }
    }
}