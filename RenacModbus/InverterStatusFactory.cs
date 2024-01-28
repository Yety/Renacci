using Renacci;

namespace RenacModbus;

public class InverterStatusFactory
{
    public InverterStatus Create(Span<byte> data)
    {
        var newStatus = new InverterStatus
        {
            Pv1Vol = data.ToDouble(0) / 10.0,
            Pv1Cur = data.ToDouble(2) / 10.0,
            Pv1Power = data.ToDouble(4),
            Pv2Vol = data.ToDouble(6) / 10.0,
            Pv2Cur = data.ToDouble(8) / 10.0,
            Pv2Power = data.ToDouble(10),
            IntTemp = data.ToDouble(32) / 10.0,
            ChargerTemp = data.ToDouble(34) / 10.0,
            BoostTemp = data.ToDouble(36) / 10.0,
            InvTemp = data.ToDouble(38) / 10.0,
            BatteryVol = data.ToDouble(40) / 10.0,
            BatteryCur = data.ToDouble(42) / 10.0,
            BatteryPower = data.ToDouble(44),
            BmsVol = data.ToDouble(46) / 10.0,
            BmsCur = data.ToDouble(48) / 10.0,
            BmsTemp = data.ToDouble(50) / 10.0,
            Soc = data.ToDouble(52),
            Soh = data.ToDouble(54),
            // BatteryStatus = data.ToInt32(80), // Assuming the use of an equivalent ToInt32 method
            // CycleTime = data.ToInt32(82), // Assuming the use of an equivalent ToInt32 method
            // AhNum = data.ToInt32(112), // Assuming the use of an equivalent ToInt32 method
            // InvState = data.ToInt32(114), // Assuming the use of an equivalent ToInt32 method
            // BmsState = data.ToInt32(116), // Assuming the use of an equivalent ToInt32 method
            // MeterState = data.ToInt32(118), // Assuming the use of an equivalent ToInt32 method
            // Alarm = GetError(120), // Assuming the existence of a GetError method
            GridVol = data.ToDouble(152) / 10.0,
            GridSVol = data.ToDouble(154) / 10.0,
            GridTVol = data.ToDouble(156) / 10.0,
            InvCur = data.ToDouble(158) / 10.0,
            InvSCur = data.ToDouble(160) / 10.0,
            InvTCur = data.ToDouble(162) / 10.0,
            InvPower = data.ToDouble(164, true),
            InvSPower = data.ToDouble(166, true),
            InvTPower = data.ToDouble(168, true),
            GridFre = data.ToDouble(170) / 100.0,
            GridSFre = data.ToDouble(172) / 100.0,
            GridTFre = data.ToDouble(174) / 100.0,
            EpsVol = data.ToDouble(176) / 10.0,
            EpsSVol = data.ToDouble(178) / 10.0,
            EpsTVol = data.ToDouble(180) / 10.0,
            EpsCur = data.ToDouble(182) / 10.0,
            EpsSCur = data.ToDouble(184) / 10.0,
            EpsTCur = data.ToDouble(186) / 10.0,
            EpsPower = data.ToDouble(188),
            EpsSPower = data.ToDouble(190),
            EpsTPower = data.ToDouble(192),
            EpsFre = data.ToDouble(194) / 100.0,
            MeterRPower = data.ToDouble(196, true),
            MeterSPower = data.ToDouble(198),
            MeterTPower = data.ToDouble(200),
            MeterPower = data.ToDouble(202),
            Meter2RPower = data.ToDouble(204),
            Meter2SPower = data.ToDouble(206),
            Meter2TPower = data.ToDouble(208),
            Meter2Power = data.ToDouble(210),
            Meter3RPower = data.ToDouble(212),
            Meter3SPower = data.ToDouble(214),
            Meter3TPower = data.ToDouble(216),
            Meter3Power = data.ToDouble(218),
            LoadRPower = data.ToDouble(220),
            LoadSPower = data.ToDouble(222),
            LoadTPower = data.ToDouble(224),
            LoadPower = data.ToDouble(226)
        };
        return newStatus;
    }
}