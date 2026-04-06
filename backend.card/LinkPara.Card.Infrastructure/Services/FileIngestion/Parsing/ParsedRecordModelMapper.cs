using LinkPara.Card.Domain.Entities.FileIngestion;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public class ParsedRecordModelMapper : IParsedRecordModelMapper
{
    public object Create(string profileKey, ParsedFileLine parsedLine)
    {
        return (profileKey, parsedLine.RecordType) switch
        {
            ("ClearingVisa", "H") => MapClearingVisaHeader(parsedLine),
            ("ClearingVisa", "D") => MapClearingVisaDetail(parsedLine),
            ("ClearingVisa", "F") => MapClearingVisaFooter(parsedLine),
            ("ClearingMsc", "H") => MapClearingMscHeader(parsedLine),
            ("ClearingMsc", "D") => MapClearingMscDetail(parsedLine),
            ("ClearingMsc", "F") => MapClearingMscFooter(parsedLine),
            ("ClearingBkm", "H") => MapClearingBkmHeader(parsedLine),
            ("ClearingBkm", "D") => MapClearingBkmDetail(parsedLine),
            ("ClearingBkm", "F") => MapClearingBkmFooter(parsedLine),
            ("CardVisa", "H") => MapCardVisaHeader(parsedLine),
            ("CardVisa", "D") => MapCardVisaDetail(parsedLine),
            ("CardVisa", "F") => MapCardVisaFooter(parsedLine),
            ("CardMsc", "H") => MapCardMscHeader(parsedLine),
            ("CardMsc", "D") => MapCardMscDetail(parsedLine),
            ("CardMsc", "F") => MapCardMscFooter(parsedLine),
            ("CardBkm", "H") => MapCardBkmHeader(parsedLine),
            ("CardBkm", "D") => MapCardBkmDetail(parsedLine),
            ("CardBkm", "F") => MapCardBkmFooter(parsedLine),
            _ => throw new InvalidOperationException($"Typed schema mapper is not defined for profile '{profileKey}' and record type '{parsedLine.RecordType}'.")
        };
    }

    private static ClearingVisaHeader MapClearingVisaHeader(ParsedFileLine line) => new()
    {
        HeaderCode = EnumValue<ClearingVisaHeaderCode>(line, nameof(ClearingVisaHeader.HeaderCode), "HeaderCode"),
        FileDate = Value(line, nameof(ClearingVisaHeader.FileDate)),
        FileNo = LongValue(line, nameof(ClearingVisaHeader.FileNo)),
        FileVersionNumber = Value(line, nameof(ClearingVisaHeader.FileVersionNumber))
    };

    private static ClearingVisaDetail MapClearingVisaDetail(ParsedFileLine line) => new()
    {
        TxnType = EnumValue<ClearingVisaTxnType>(line, nameof(ClearingVisaDetail.TxnType)),
        IoDate = Value(line, nameof(ClearingVisaDetail.IoDate)),
        IoFlag = EnumValue<ClearingVisaIoFlag>(line, nameof(ClearingVisaDetail.IoFlag)),
        OceanTxnGuid = LongValue(line, nameof(ClearingVisaDetail.OceanTxnGuid)),
        ClrNo = LongValue(line, nameof(ClearingVisaDetail.ClrNo)),
        Rrn = Value(line, nameof(ClearingVisaDetail.Rrn)),
        Arn = Value(line, nameof(ClearingVisaDetail.Arn)),
        ReasonCode = Value(line, nameof(ClearingVisaDetail.ReasonCode)),
        Reserved = Value(line, nameof(ClearingVisaDetail.Reserved)),
        ProvisionCode = Value(line, nameof(ClearingVisaDetail.ProvisionCode)),
        CardNo = Value(line, nameof(ClearingVisaDetail.CardNo)),
        CardDci = NullableEnumValue<ClearingVisaCardDci>(line, nameof(ClearingVisaDetail.CardDci), "CardDCI"),
        MccCode = Value(line, nameof(ClearingVisaDetail.MccCode), "MCCCode"),
        Tc = Value(line, nameof(ClearingVisaDetail.Tc), "TC"),
        UsageCode = Value(line, nameof(ClearingVisaDetail.UsageCode)),
        DisputeCode = EnumValue<ClearingVisaDisputeCode>(line, nameof(ClearingVisaDetail.DisputeCode)),
        ControlStat = EnumValue<ClearingVisaControlStat>(line, nameof(ClearingVisaDetail.ControlStat)),
        SourceAmount = DecimalValue(line, nameof(ClearingVisaDetail.SourceAmount)),
        SourceCurrency = IntValue(line, nameof(ClearingVisaDetail.SourceCurrency)),
        DestinationAmount = DecimalValue(line, nameof(ClearingVisaDetail.DestinationAmount)),
        DestinationCurrency = IntValue(line, nameof(ClearingVisaDetail.DestinationCurrency)),
        CashbackAmount = DecimalValue(line, nameof(ClearingVisaDetail.CashbackAmount)),
        ReimbursementAmount = DecimalValue(line, nameof(ClearingVisaDetail.ReimbursementAmount)),
        ReimbursementAttribute = Value(line, nameof(ClearingVisaDetail.ReimbursementAttribute)),
        MicrofilmNumber = IntValue(line, nameof(ClearingVisaDetail.MicrofilmNumber)),
        MerchantCity = Value(line, nameof(ClearingVisaDetail.MerchantCity)),
        MerchantName = Value(line, nameof(ClearingVisaDetail.MerchantName)),
        CardAcceptorId = Value(line, nameof(ClearingVisaDetail.CardAcceptorId)),
        TxnDate = IntValue(line, nameof(ClearingVisaDetail.TxnDate)),
        TxnTime = IntValue(line, nameof(ClearingVisaDetail.TxnTime)),
        FileId = Value(line, nameof(ClearingVisaDetail.FileId))
    };

    private static ClearingVisaFooter MapClearingVisaFooter(ParsedFileLine line) => new()
    {
        FooterCode = EnumValue<ClearingVisaFooterCode>(line, nameof(ClearingVisaFooter.FooterCode)),
        FileDate = Value(line, nameof(ClearingVisaFooter.FileDate)),
        TxnCount = LongValue(line, nameof(ClearingVisaFooter.TxnCount))
    };

    private static ClearingMscHeader MapClearingMscHeader(ParsedFileLine line) => new()
    {
        HeaderCode = EnumValue<ClearingMscHeaderCode>(line, nameof(ClearingMscHeader.HeaderCode)),
        FileDate = Value(line, nameof(ClearingMscHeader.FileDate)),
        FileNo = LongValue(line, nameof(ClearingMscHeader.FileNo)),
        FileVersionNumber = Value(line, nameof(ClearingMscHeader.FileVersionNumber))
    };

    private static ClearingMscDetail MapClearingMscDetail(ParsedFileLine line) => new()
    {
        TxnType = EnumValue<ClearingMscTxnType>(line, nameof(ClearingMscDetail.TxnType)),
        IoDate = Value(line, nameof(ClearingMscDetail.IoDate)),
        IoFlag = EnumValue<ClearingMscIoFlag>(line, nameof(ClearingMscDetail.IoFlag)),
        OceanTxnGuid = LongValue(line, nameof(ClearingMscDetail.OceanTxnGuid)),
        ClrNo = LongValue(line, nameof(ClearingMscDetail.ClrNo)),
        Rrn = Value(line, nameof(ClearingMscDetail.Rrn)),
        Arn = Value(line, nameof(ClearingMscDetail.Arn)),
        ReasonCode = Value(line, nameof(ClearingMscDetail.ReasonCode)),
        Reserved = Value(line, nameof(ClearingMscDetail.Reserved)),
        ProvisionCode = Value(line, nameof(ClearingMscDetail.ProvisionCode)),
        CardNo = Value(line, nameof(ClearingMscDetail.CardNo)),
        CardDci = NullableEnumValue<ClearingMscCardDci>(line, nameof(ClearingMscDetail.CardDci), "CardDCI"),
        MccCode = Value(line, nameof(ClearingMscDetail.MccCode), "MCCCode"),
        Mtid = Value(line, nameof(ClearingMscDetail.Mtid)),
        FunctionCode = Value(line, nameof(ClearingMscDetail.FunctionCode)),
        ProcessCode = Value(line, nameof(ClearingMscDetail.ProcessCode)),
        ReversalIndicator = Value(line, nameof(ClearingMscDetail.ReversalIndicator)),
        DisputeCode = EnumValue<ClearingMscDisputeCode>(line, nameof(ClearingMscDetail.DisputeCode)),
        ControlStat = EnumValue<ClearingMscControlStat>(line, nameof(ClearingMscDetail.ControlStat)),
        SourceAmount = DecimalValue(line, nameof(ClearingMscDetail.SourceAmount)),
        SourceCurrency = IntValue(line, nameof(ClearingMscDetail.SourceCurrency)),
        DestinationAmount = DecimalValue(line, nameof(ClearingMscDetail.DestinationAmount)),
        DestinationCurrency = IntValue(line, nameof(ClearingMscDetail.DestinationCurrency)),
        CashbackAmount = DecimalValue(line, nameof(ClearingMscDetail.CashbackAmount)),
        ReimbursementAmount = DecimalValue(line, nameof(ClearingMscDetail.ReimbursementAmount)),
        ReimbursementAttribute = Value(line, nameof(ClearingMscDetail.ReimbursementAttribute)),
        AncillaryTransactionCode = Value(line, nameof(ClearingMscDetail.AncillaryTransactionCode)),
        AncillaryTransactionAmount = Value(line, nameof(ClearingMscDetail.AncillaryTransactionAmount)),
        MicrofilmNumber = IntValue(line, nameof(ClearingMscDetail.MicrofilmNumber)),
        MerchantCity = Value(line, nameof(ClearingMscDetail.MerchantCity)),
        MerchantName = Value(line, nameof(ClearingMscDetail.MerchantName)),
        CardAcceptorId = Value(line, nameof(ClearingMscDetail.CardAcceptorId)),
        TxnDate = IntValue(line, nameof(ClearingMscDetail.TxnDate)),
        TxnTime = IntValue(line, nameof(ClearingMscDetail.TxnTime)),
        FileId = Value(line, nameof(ClearingMscDetail.FileId))
    };

    private static ClearingMscFooter MapClearingMscFooter(ParsedFileLine line) => new()
    {
        FooterCode = EnumValue<ClearingMscFooterCode>(line, nameof(ClearingMscFooter.FooterCode)),
        FileDate = Value(line, nameof(ClearingMscFooter.FileDate)),
        TxnCount = LongValue(line, nameof(ClearingMscFooter.TxnCount))
    };

    private static ClearingBkmHeader MapClearingBkmHeader(ParsedFileLine line) => new()
    {
        HeaderCode = EnumValue<ClearingBkmHeaderCode>(line, nameof(ClearingBkmHeader.HeaderCode)),
        FileDate = Value(line, nameof(ClearingBkmHeader.FileDate)),
        FileNo = LongValue(line, nameof(ClearingBkmHeader.FileNo)),
        FileVersionNumber = Value(line, nameof(ClearingBkmHeader.FileVersionNumber))
    };

    private static ClearingBkmDetail MapClearingBkmDetail(ParsedFileLine line) => new()
    {
        TxnType = EnumValue<ClearingBkmTxnType>(line, nameof(ClearingBkmDetail.TxnType)),
        IoDate = Value(line, nameof(ClearingBkmDetail.IoDate)),
        IoFlag = EnumValue<ClearingBkmIoFlag>(line, nameof(ClearingBkmDetail.IoFlag)),
        OceanTxnGuid = LongValue(line, nameof(ClearingBkmDetail.OceanTxnGuid)),
        ClrNo = LongValue(line, nameof(ClearingBkmDetail.ClrNo)),
        Rrn = Value(line, nameof(ClearingBkmDetail.Rrn)),
        Arn = Value(line, nameof(ClearingBkmDetail.Arn)),
        ReasonCode = Value(line, nameof(ClearingBkmDetail.ReasonCode)),
        Reserved = Value(line, nameof(ClearingBkmDetail.Reserved)),
        ProvisionCode = Value(line, nameof(ClearingBkmDetail.ProvisionCode)),
        CardNo = Value(line, nameof(ClearingBkmDetail.CardNo)),
        CardDci = NullableEnumValue<ClearingBkmCardDci>(line, nameof(ClearingBkmDetail.CardDci), "CardDCI"),
        MccCode = Value(line, nameof(ClearingBkmDetail.MccCode), "MCCCode"),
        Mtid = Value(line, nameof(ClearingBkmDetail.Mtid)),
        FunctionCode = Value(line, nameof(ClearingBkmDetail.FunctionCode)),
        ProcessCode = Value(line, nameof(ClearingBkmDetail.ProcessCode)),
        DisputeCode = EnumValue<ClearingBkmDisputeCode>(line, nameof(ClearingBkmDetail.DisputeCode)),
        ControlStat = EnumValue<ClearingBkmControlStat>(line, nameof(ClearingBkmDetail.ControlStat)),
        SourceAmount = DecimalValue(line, nameof(ClearingBkmDetail.SourceAmount)),
        SourceCurrency = IntValue(line, nameof(ClearingBkmDetail.SourceCurrency)),
        DestinationAmount = DecimalValue(line, nameof(ClearingBkmDetail.DestinationAmount)),
        DestinationCurrency = IntValue(line, nameof(ClearingBkmDetail.DestinationCurrency)),
        CashbackAmount = DecimalValue(line, nameof(ClearingBkmDetail.CashbackAmount)),
        ReimbursementAmount = DecimalValue(line, nameof(ClearingBkmDetail.ReimbursementAmount)),
        ReimbursementAttribute = Value(line, nameof(ClearingBkmDetail.ReimbursementAttribute)),
        MicrofilmNumber = IntValue(line, nameof(ClearingBkmDetail.MicrofilmNumber)),
        MerchantCity = Value(line, nameof(ClearingBkmDetail.MerchantCity)),
        MerchantName = Value(line, nameof(ClearingBkmDetail.MerchantName)),
        CardAcceptorId = Value(line, nameof(ClearingBkmDetail.CardAcceptorId)),
        TxnDate = IntValue(line, nameof(ClearingBkmDetail.TxnDate)),
        TxnTime = IntValue(line, nameof(ClearingBkmDetail.TxnTime)),
        FileId = Value(line, nameof(ClearingBkmDetail.FileId))
    };

    private static ClearingBkmFooter MapClearingBkmFooter(ParsedFileLine line) => new()
    {
        FooterCode = EnumValue<ClearingBkmFooterCode>(line, nameof(ClearingBkmFooter.FooterCode)),
        FileDate = Value(line, nameof(ClearingBkmFooter.FileDate)),
        TxnCount = LongValue(line, nameof(ClearingBkmFooter.TxnCount))
    };

    private static CardVisaHeader MapCardVisaHeader(ParsedFileLine line) => new()
    {
        HeaderCode = EnumValue<CardVisaHeaderCode>(line, nameof(CardVisaHeader.HeaderCode)),
        FileDate = Value(line, nameof(CardVisaHeader.FileDate)),
        FileNo = Value(line, nameof(CardVisaHeader.FileNo)),
        FileVersionNumber = Value(line, nameof(CardVisaHeader.FileVersionNumber))
    };

    private static CardVisaDetail MapCardVisaDetail(ParsedFileLine line) => new()
    {
        TransactionDate = IntValue(line, nameof(CardVisaDetail.TransactionDate)),
        TransactionTime = IntValue(line, nameof(CardVisaDetail.TransactionTime)),
        ValueDate = IntValue(line, nameof(CardVisaDetail.ValueDate)),
        EndOfDayDate = IntValue(line, nameof(CardVisaDetail.EndOfDayDate)),
        CardNo = Value(line, nameof(CardVisaDetail.CardNo)),
        OceanTxnGuid = LongValue(line, nameof(CardVisaDetail.OceanTxnGuid)),
        OceanMainTxnGuid = LongValue(line, nameof(CardVisaDetail.OceanMainTxnGuid)),
        BranchId = Value(line, nameof(CardVisaDetail.BranchId), "BranchID"),
        Rrn = Value(line, nameof(CardVisaDetail.Rrn), "RRN"),
        Arn = Value(line, nameof(CardVisaDetail.Arn), "ARN"),
        ProvisionCode = Value(line, nameof(CardVisaDetail.ProvisionCode)),
        Stan = IntValue(line, nameof(CardVisaDetail.Stan), "Stan"),
        MemberRefNo = Value(line, nameof(CardVisaDetail.MemberRefNo)),
        TraceId = LongValue(line, nameof(CardVisaDetail.TraceId), "TraceID"),
        Otc = IntValue(line, nameof(CardVisaDetail.Otc), "Otc"),
        Ots = IntValue(line, nameof(CardVisaDetail.Ots), "Ots"),
        TxnInstallType = EnumValue<CardVisaTxnInstallType>(line, nameof(CardVisaDetail.TxnInstallType)),
        BankingTxnCode = Value(line, nameof(CardVisaDetail.BankingTxnCode)),
        TxnDescription = Value(line, nameof(CardVisaDetail.TxnDescription)),
        MerchantName = Value(line, nameof(CardVisaDetail.MerchantName)),
        MerchantCity = Value(line, nameof(CardVisaDetail.MerchantCity)),
        MerchantState = Value(line, nameof(CardVisaDetail.MerchantState)),
        MerchantCountry = Value(line, nameof(CardVisaDetail.MerchantCountry)),
        FinancialType = EnumValue<CardVisaFinancialType>(line, nameof(CardVisaDetail.FinancialType)),
        TxnEffect = EnumValue<CardVisaTxnEffect>(line, nameof(CardVisaDetail.TxnEffect)),
        TxnSource = EnumValue<CardVisaTxnSource>(line, nameof(CardVisaDetail.TxnSource)),
        TxnRegion = EnumValue<CardVisaTxnRegion>(line, nameof(CardVisaDetail.TxnRegion)),
        TerminalType = EnumValue<CardVisaTerminalType>(line, nameof(CardVisaDetail.TerminalType)),
        ChannelCode = EnumValue<CardVisaChannelCode>(line, nameof(CardVisaDetail.ChannelCode)),
        TerminalId = Value(line, nameof(CardVisaDetail.TerminalId)),
        MerchantId = Value(line, nameof(CardVisaDetail.MerchantId)),
        Mcc = IntValue(line, nameof(CardVisaDetail.Mcc), "MCC", "Mcc"),
        AcquirerId = IntValue(line, nameof(CardVisaDetail.AcquirerId)),
        SecurityLevelIndicator = IntValue(line, nameof(CardVisaDetail.SecurityLevelIndicator)),
        IsTxnSettle = EnumValue<CardVisaIsTxnSettle>(line, nameof(CardVisaDetail.IsTxnSettle)),
        TxnStat = EnumValue<CardVisaTxnStat>(line, nameof(CardVisaDetail.TxnStat)),
        ResponseCode = Value(line, nameof(CardVisaDetail.ResponseCode)),
        IsSuccessfulTxn = EnumValue<CardVisaIsSuccessfulTxn>(line, nameof(CardVisaDetail.IsSuccessfulTxn)),
        TxnOrigin = EnumValue<CardVisaTxnOrigin>(line, nameof(CardVisaDetail.TxnOrigin)),
        InstallCount = IntValue(line, nameof(CardVisaDetail.InstallCount)),
        InstallOrder = IntValue(line, nameof(CardVisaDetail.InstallOrder)),
        OperatorCode = Value(line, nameof(CardVisaDetail.OperatorCode)),
        OriginalAmount = DecimalValue(line, nameof(CardVisaDetail.OriginalAmount)),
        OriginalCurrency = IntValue(line, nameof(CardVisaDetail.OriginalCurrency)),
        SettlementAmount = DecimalValue(line, nameof(CardVisaDetail.SettlementAmount)),
        SettlementCurrency = IntValue(line, nameof(CardVisaDetail.SettlementCurrency)),
        CardHolderBillingAmount = DecimalValue(line, nameof(CardVisaDetail.CardHolderBillingAmount)),
        CardHolderBillingCurrency = IntValue(line, nameof(CardVisaDetail.CardHolderBillingCurrency)),
        BillingAmount = DecimalValue(line, nameof(CardVisaDetail.BillingAmount)),
        BillingCurrency = IntValue(line, nameof(CardVisaDetail.BillingCurrency)),
        Tax1 = DecimalValue(line, nameof(CardVisaDetail.Tax1)),
        Tax2 = DecimalValue(line, nameof(CardVisaDetail.Tax2)),
        CashbackAmount = DecimalValue(line, nameof(CardVisaDetail.CashbackAmount)),
        SurchargeAmount = DecimalValue(line, nameof(CardVisaDetail.SurchargeAmount)),
        PointType = Value(line, nameof(CardVisaDetail.PointType)),
        BcPoint = DecimalValue(line, nameof(CardVisaDetail.BcPoint)),
        McPoint = DecimalValue(line, nameof(CardVisaDetail.McPoint)),
        CcPoint = DecimalValue(line, nameof(CardVisaDetail.CcPoint)),
        BcPointAmount = DecimalValue(line, nameof(CardVisaDetail.BcPointAmount)),
        McPointAmount = DecimalValue(line, nameof(CardVisaDetail.McPointAmount)),
        CcPointAmount = DecimalValue(line, nameof(CardVisaDetail.CcPointAmount))
    };

    private static CardVisaFooter MapCardVisaFooter(ParsedFileLine line) => new()
    {
        FooterCode = EnumValue<CardVisaFooterCode>(line, nameof(CardVisaFooter.FooterCode)),
        FileDate = Value(line, nameof(CardVisaFooter.FileDate)),
        TxnCount = LongValue(line, nameof(CardVisaFooter.TxnCount))
    };

    private static CardMscHeader MapCardMscHeader(ParsedFileLine line) => new()
    {
        HeaderCode = EnumValue<CardMscHeaderCode>(line, nameof(CardMscHeader.HeaderCode)),
        FileDate = Value(line, nameof(CardMscHeader.FileDate)),
        FileNo = Value(line, nameof(CardMscHeader.FileNo)),
        FileVersionNumber = Value(line, nameof(CardMscHeader.FileVersionNumber))
    };

    private static CardMscDetail MapCardMscDetail(ParsedFileLine line) => new()
    {
        TransactionDate = IntValue(line, nameof(CardMscDetail.TransactionDate)),
        TransactionTime = IntValue(line, nameof(CardMscDetail.TransactionTime)),
        ValueDate = IntValue(line, nameof(CardMscDetail.ValueDate)),
        EndOfDayDate = IntValue(line, nameof(CardMscDetail.EndOfDayDate)),
        CardNo = Value(line, nameof(CardMscDetail.CardNo)),
        OceanTxnGuid = LongValue(line, nameof(CardMscDetail.OceanTxnGuid)),
        OceanMainTxnGuid = LongValue(line, nameof(CardMscDetail.OceanMainTxnGuid)),
        BranchId = Value(line, nameof(CardMscDetail.BranchId), "BranchID"),
        Rrn = Value(line, nameof(CardMscDetail.Rrn), "RRN"),
        Arn = Value(line, nameof(CardMscDetail.Arn), "ARN"),
        ProvisionCode = Value(line, nameof(CardMscDetail.ProvisionCode)),
        Stan = IntValue(line, nameof(CardMscDetail.Stan)),
        MemberRefNo = Value(line, nameof(CardMscDetail.MemberRefNo)),
        TraceId = LongValue(line, nameof(CardMscDetail.TraceId), "TraceID"),
        Otc = IntValue(line, nameof(CardMscDetail.Otc)),
        Ots = IntValue(line, nameof(CardMscDetail.Ots)),
        TxnInstallType = EnumValue<CardMscTxnInstallType>(line, nameof(CardMscDetail.TxnInstallType)),
        BankingTxnCode = Value(line, nameof(CardMscDetail.BankingTxnCode)),
        TxnDescription = Value(line, nameof(CardMscDetail.TxnDescription)),
        MerchantName = Value(line, nameof(CardMscDetail.MerchantName)),
        MerchantCity = Value(line, nameof(CardMscDetail.MerchantCity)),
        MerchantState = Value(line, nameof(CardMscDetail.MerchantState)),
        MerchantCountry = Value(line, nameof(CardMscDetail.MerchantCountry)),
        FinancialType = EnumValue<CardMscFinancialType>(line, nameof(CardMscDetail.FinancialType)),
        TxnEffect = EnumValue<CardMscTxnEffect>(line, nameof(CardMscDetail.TxnEffect)),
        TxnSource = EnumValue<CardMscTxnSource>(line, nameof(CardMscDetail.TxnSource)),
        TxnRegion = EnumValue<CardMscTxnRegion>(line, nameof(CardMscDetail.TxnRegion)),
        TerminalType = EnumValue<CardMscTerminalType>(line, nameof(CardMscDetail.TerminalType)),
        ChannelCode = EnumValue<CardMscChannelCode>(line, nameof(CardMscDetail.ChannelCode)),
        TerminalId = Value(line, nameof(CardMscDetail.TerminalId)),
        MerchantId = Value(line, nameof(CardMscDetail.MerchantId)),
        Mcc = IntValue(line, nameof(CardMscDetail.Mcc), "MCC", "Mcc"),
        AcquirerId = IntValue(line, nameof(CardMscDetail.AcquirerId)),
        SecurityLevelIndicator = IntValue(line, nameof(CardMscDetail.SecurityLevelIndicator)),
        IsTxnSettle = EnumValue<CardMscIsTxnSettle>(line, nameof(CardMscDetail.IsTxnSettle)),
        TxnStat = EnumValue<CardMscTxnStat>(line, nameof(CardMscDetail.TxnStat)),
        ResponseCode = Value(line, nameof(CardMscDetail.ResponseCode)),
        IsSuccessfulTxn = EnumValue<CardMscIsSuccessfulTxn>(line, nameof(CardMscDetail.IsSuccessfulTxn)),
        TxnOrigin = EnumValue<CardMscTxnOrigin>(line, nameof(CardMscDetail.TxnOrigin)),
        InstallCount = IntValue(line, nameof(CardMscDetail.InstallCount)),
        InstallOrder = IntValue(line, nameof(CardMscDetail.InstallOrder)),
        OperatorCode = Value(line, nameof(CardMscDetail.OperatorCode)),
        OriginalAmount = DecimalValue(line, nameof(CardMscDetail.OriginalAmount)),
        OriginalCurrency = IntValue(line, nameof(CardMscDetail.OriginalCurrency)),
        SettlementAmount = DecimalValue(line, nameof(CardMscDetail.SettlementAmount)),
        SettlementCurrency = IntValue(line, nameof(CardMscDetail.SettlementCurrency)),
        CardHolderBillingAmount = DecimalValue(line, nameof(CardMscDetail.CardHolderBillingAmount)),
        CardHolderBillingCurrency = IntValue(line, nameof(CardMscDetail.CardHolderBillingCurrency)),
        BillingAmount = DecimalValue(line, nameof(CardMscDetail.BillingAmount)),
        BillingCurrency = IntValue(line, nameof(CardMscDetail.BillingCurrency)),
        Tax1 = DecimalValue(line, nameof(CardMscDetail.Tax1)),
        Tax2 = DecimalValue(line, nameof(CardMscDetail.Tax2)),
        CashbackAmount = DecimalValue(line, nameof(CardMscDetail.CashbackAmount)),
        SurchargeAmount = DecimalValue(line, nameof(CardMscDetail.SurchargeAmount)),
        PointType = Value(line, nameof(CardMscDetail.PointType)),
        BcPoint = DecimalValue(line, nameof(CardMscDetail.BcPoint)),
        McPoint = DecimalValue(line, nameof(CardMscDetail.McPoint)),
        CcPoint = DecimalValue(line, nameof(CardMscDetail.CcPoint)),
        BcPointAmount = DecimalValue(line, nameof(CardMscDetail.BcPointAmount)),
        McPointAmount = DecimalValue(line, nameof(CardMscDetail.McPointAmount)),
        CcPointAmount = DecimalValue(line, nameof(CardMscDetail.CcPointAmount))
    };

    private static CardMscFooter MapCardMscFooter(ParsedFileLine line) => new()
    {
        FooterCode = EnumValue<CardMscFooterCode>(line, nameof(CardMscFooter.FooterCode)),
        FileDate = Value(line, nameof(CardMscFooter.FileDate)),
        TxnCount = LongValue(line, nameof(CardMscFooter.TxnCount))
    };

    private static CardBkmHeader MapCardBkmHeader(ParsedFileLine line) => new()
    {
        HeaderCode = EnumValue<CardBkmHeaderCode>(line, nameof(CardBkmHeader.HeaderCode)),
        FileDate = Value(line, nameof(CardBkmHeader.FileDate)),
        FileNo = Value(line, nameof(CardBkmHeader.FileNo)),
        FileVersionNumber = Value(line, nameof(CardBkmHeader.FileVersionNumber))
    };

    private static CardBkmDetail MapCardBkmDetail(ParsedFileLine line) => new()
    {
        TransactionDate = IntValue(line, nameof(CardBkmDetail.TransactionDate)),
        TransactionTime = IntValue(line, nameof(CardBkmDetail.TransactionTime)),
        ValueDate = IntValue(line, nameof(CardBkmDetail.ValueDate)),
        EndOfDayDate = IntValue(line, nameof(CardBkmDetail.EndOfDayDate)),
        CardNo = Value(line, nameof(CardBkmDetail.CardNo)),
        OceanTxnGuid = LongValue(line, nameof(CardBkmDetail.OceanTxnGuid)),
        OceanMainTxnGuid = LongValue(line, nameof(CardBkmDetail.OceanMainTxnGuid)),
        BranchId = Value(line, nameof(CardBkmDetail.BranchId), "BranchID"),
        Rrn = Value(line, nameof(CardBkmDetail.Rrn), "RRN"),
        Arn = Value(line, nameof(CardBkmDetail.Arn), "ARN"),
        ProvisionCode = Value(line, nameof(CardBkmDetail.ProvisionCode)),
        Stan = IntValue(line, nameof(CardBkmDetail.Stan)),
        MemberRefNo = Value(line, nameof(CardBkmDetail.MemberRefNo)),
        TraceId = LongValue(line, nameof(CardBkmDetail.TraceId), "TraceID"),
        Otc = IntValue(line, nameof(CardBkmDetail.Otc)),
        Ots = IntValue(line, nameof(CardBkmDetail.Ots)),
        TxnInstallType = EnumValue<CardBkmTxnInstallType>(line, nameof(CardBkmDetail.TxnInstallType)),
        BankingTxnCode = Value(line, nameof(CardBkmDetail.BankingTxnCode)),
        TxnDescription = Value(line, nameof(CardBkmDetail.TxnDescription)),
        MerchantName = Value(line, nameof(CardBkmDetail.MerchantName)),
        MerchantCity = Value(line, nameof(CardBkmDetail.MerchantCity)),
        MerchantState = Value(line, nameof(CardBkmDetail.MerchantState)),
        MerchantCountry = Value(line, nameof(CardBkmDetail.MerchantCountry)),
        FinancialType = EnumValue<CardBkmFinancialType>(line, nameof(CardBkmDetail.FinancialType)),
        TxnEffect = EnumValue<CardBkmTxnEffect>(line, nameof(CardBkmDetail.TxnEffect)),
        TxnSource = EnumValue<CardBkmTxnSource>(line, nameof(CardBkmDetail.TxnSource)),
        TxnRegion = EnumValue<CardBkmTxnRegion>(line, nameof(CardBkmDetail.TxnRegion)),
        TerminalType = EnumValue<CardBkmTerminalType>(line, nameof(CardBkmDetail.TerminalType)),
        ChannelCode = EnumValue<CardBkmChannelCode>(line, nameof(CardBkmDetail.ChannelCode)),
        TerminalId = Value(line, nameof(CardBkmDetail.TerminalId)),
        MerchantId = Value(line, nameof(CardBkmDetail.MerchantId)),
        Mcc = IntValue(line, nameof(CardBkmDetail.Mcc), "MCC", "Mcc"),
        AcquirerId = IntValue(line, nameof(CardBkmDetail.AcquirerId)),
        SecurityLevelIndicator = IntValue(line, nameof(CardBkmDetail.SecurityLevelIndicator)),
        IsTxnSettle = EnumValue<CardBkmIsTxnSettle>(line, nameof(CardBkmDetail.IsTxnSettle)),
        TxnStat = EnumValue<CardBkmTxnStat>(line, nameof(CardBkmDetail.TxnStat)),
        ResponseCode = Value(line, nameof(CardBkmDetail.ResponseCode)),
        IsSuccessfulTxn = EnumValue<CardBkmIsSuccessfulTxn>(line, nameof(CardBkmDetail.IsSuccessfulTxn)),
        TxnOrigin = EnumValue<CardBkmTxnOrigin>(line, nameof(CardBkmDetail.TxnOrigin)),
        InstallCount = IntValue(line, nameof(CardBkmDetail.InstallCount)),
        InstallOrder = IntValue(line, nameof(CardBkmDetail.InstallOrder)),
        OperatorCode = Value(line, nameof(CardBkmDetail.OperatorCode)),
        OriginalAmount = DecimalValue(line, nameof(CardBkmDetail.OriginalAmount)),
        OriginalCurrency = IntValue(line, nameof(CardBkmDetail.OriginalCurrency)),
        SettlementAmount = DecimalValue(line, nameof(CardBkmDetail.SettlementAmount)),
        SettlementCurrency = IntValue(line, nameof(CardBkmDetail.SettlementCurrency)),
        CardHolderBillingAmount = DecimalValue(line, nameof(CardBkmDetail.CardHolderBillingAmount)),
        CardHolderBillingCurrency = IntValue(line, nameof(CardBkmDetail.CardHolderBillingCurrency)),
        BillingAmount = DecimalValue(line, nameof(CardBkmDetail.BillingAmount)),
        BillingCurrency = IntValue(line, nameof(CardBkmDetail.BillingCurrency)),
        Tax1 = DecimalValue(line, nameof(CardBkmDetail.Tax1)),
        Tax2 = DecimalValue(line, nameof(CardBkmDetail.Tax2)),
        CashbackAmount = DecimalValue(line, nameof(CardBkmDetail.CashbackAmount)),
        SurchargeAmount = DecimalValue(line, nameof(CardBkmDetail.SurchargeAmount)),
        PointType = Value(line, nameof(CardBkmDetail.PointType)),
        BcPoint = DecimalValue(line, nameof(CardBkmDetail.BcPoint)),
        McPoint = DecimalValue(line, nameof(CardBkmDetail.McPoint)),
        CcPoint = DecimalValue(line, nameof(CardBkmDetail.CcPoint)),
        BcPointAmount = DecimalValue(line, nameof(CardBkmDetail.BcPointAmount)),
        McPointAmount = DecimalValue(line, nameof(CardBkmDetail.McPointAmount)),
        CcPointAmount = DecimalValue(line, nameof(CardBkmDetail.CcPointAmount))
    };

    private static CardBkmFooter MapCardBkmFooter(ParsedFileLine line) => new()
    {
        FooterCode = EnumValue<CardBkmFooterCode>(line, nameof(CardBkmFooter.FooterCode)),
        FileDate = Value(line, nameof(CardBkmFooter.FileDate)),
        TxnCount = LongValue(line, nameof(CardBkmFooter.TxnCount))
    };

    private static string Value(ParsedFileLine line, string key, params string[] aliases)
    {
        if (line.Fields.TryGetValue(key, out var value))
            return value.Trim();

        foreach (var alias in aliases)
        {
            if (line.Fields.TryGetValue(alias, out value))
                return value.Trim();
        }

        return string.Empty;
    }

    private static decimal DecimalValue(ParsedFileLine line, string key, params string[] aliases)
    {
        var rawValue = Value(line, key, aliases);
        if (string.IsNullOrWhiteSpace(rawValue))
            return default;

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
            return invariantValue;

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var trValue))
            return trValue;

        throw new InvalidOperationException($"Decimal value '{rawValue}' is not valid for '{key}'.");
    }

    private static int IntValue(ParsedFileLine line, string key, params string[] aliases)
    {
        var rawValue = Value(line, key, aliases);
        if (string.IsNullOrWhiteSpace(rawValue))
            return default;

        if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var invariantValue))
            return invariantValue;

        if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.GetCultureInfo("tr-TR"), out var trValue))
            return trValue;

        throw new InvalidOperationException($"Int value '{rawValue}' is not valid for '{key}'.");
    }

    private static long LongValue(ParsedFileLine line, string key, params string[] aliases)
    {
        var rawValue = Value(line, key, aliases);
        if (string.IsNullOrWhiteSpace(rawValue))
            return default;

        if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var invariantValue))
            return invariantValue;

        if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.GetCultureInfo("tr-TR"), out var trValue))
            return trValue;

        throw new InvalidOperationException($"Long value '{rawValue}' is not valid for '{key}'.");
    }

    private static TEnum EnumValue<TEnum>(ParsedFileLine line, string key, params string[] aliases)
        where TEnum : struct, Enum
    {
        var rawValue = Value(line, key, aliases);
        if (string.IsNullOrWhiteSpace(rawValue))
            return default;

        return ParseEnumMember<TEnum>(rawValue);
    }

    private static TEnum? NullableEnumValue<TEnum>(ParsedFileLine line, string key, params string[] aliases)
        where TEnum : struct, Enum
    {
        var rawValue = Value(line, key, aliases);
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        return ParseEnumMember<TEnum>(rawValue);
    }

    private static TEnum ParseEnumMember<TEnum>(string rawValue)
        where TEnum : struct, Enum
    {
        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMember?.Value == rawValue)
                return (TEnum)field.GetValue(null)!;

            if (field.Name.Equals(rawValue, StringComparison.OrdinalIgnoreCase))
                return (TEnum)field.GetValue(null)!;
        }

        throw new InvalidOperationException($"Enum value '{rawValue}' is not valid for '{typeof(TEnum).Name}'.");
    }
}
