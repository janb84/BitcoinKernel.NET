using BitcoinKernel;
using BitcoinKernel.Primitives;
using BitcoinKernel.Chain;
using BitcoinKernel.Exceptions;
using BitcoinKernel.ScriptVerification;
using BitcoinKernel.Interop.Enums;
using BitcoinKernel.TestHandler.Protocol;
using BitcoinKernel.TestHandler.Registry;

namespace BitcoinKernel.TestHandler.Handlers;

/// <summary>
/// Routes all incoming method calls to the appropriate handler and manages the object registry.
/// Uses BitcoinKernel managed types throughout.
/// </summary>
public sealed class MethodDispatcher : IDisposable
{
    private readonly ObjectRegistry _registry = new();
    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _registry.Dispose();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private T Get<T>(string refName) => _registry.Get<T>(refName);

    private T GetVal<T>(string refName) => _registry.Get<NonOwningRef<T>>(refName).Value;

    private static Response RefError(string id) => Responses.EmptyError(id);

    // ── Context ───────────────────────────────────────────────────────────────

    public Response ContextCreate(string id, string? refName, BtckContextCreateParams p)
    {
        if (refName == null) return RefError(id);

        try
        {
            var chainType = ParseChainType(p.ChainParameters?.ChainType ?? string.Empty);
            using var chainParams = new ChainParameters(chainType);
            using var options = new KernelContextOptions().SetChainParams(chainParams);
            var context = new KernelContext(options);
            _registry.Register(refName, context);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response ContextDestroy(string id, BtckContextDestroyParams p)
    {
        if (p.Context?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Chainstate Manager ────────────────────────────────────────────────────

    public Response ChainstateManagerCreate(string id, string? refName, BtckChainstateManagerCreateParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Context?.Ref is not { } ctxRef) return RefError(id);

        try
        {
            var context = Get<KernelContext>(ctxRef);

            var tempDir = Path.Combine(Path.GetTempPath(), $"btck_handler_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var blocksDir = Path.Combine(tempDir, "blocks");
            Directory.CreateDirectory(blocksDir);

            using var chainParams = new ChainParameters(ChainType.REGTEST);
            using var managerOptions = new ChainstateManagerOptions(context, tempDir, blocksDir)
                .SetBlockTreeDbInMemory(true)
                .SetChainstateDbInMemory(true);

            var manager = new ChainstateManager(context, chainParams, managerOptions);
            _registry.Register(refName, new ChainstateManagerWithTempDir(manager, tempDir));
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response ChainstateManagerGetActiveChain(string id, string? refName, BtckChainstateManagerGetActiveChainParams p)
    {
        if (refName == null) return RefError(id);
        if (p.ChainstateManager?.Ref is not { } csmRef) return RefError(id);

        var manager = Get<ChainstateManagerWithTempDir>(csmRef).Manager;
        var chain = manager.GetActiveChain();
        _registry.Register(refName, new NonOwningRef<BitcoinKernel.Chain.Chain>(chain));
        return Responses.Ref(id, refName);
    }

    public Response ChainstateManagerProcessBlock(string id, BtckChainstateManagerProcessBlockParams p)
    {
        if (p.ChainstateManager?.Ref is not { } csmRef) return RefError(id);
        if (p.Block?.Ref is not { } blockRef) return RefError(id);

        try
        {
            var manager = Get<ChainstateManagerWithTempDir>(csmRef).Manager;
            var block = Get<Block>(blockRef);
            bool isNew = manager.ProcessBlock(block);
            return Responses.Ok(id, new { new_block = isNew });
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response ChainstateManagerDestroy(string id, BtckChainstateManagerDestroyParams p)
    {
        if (p.ChainstateManager?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Chain ─────────────────────────────────────────────────────────────────

    public Response ChainGetHeight(string id, BtckChainGetHeightParams p)
    {
        if (p.Chain?.Ref is not { } chainRef) return RefError(id);
        return Responses.Ok(id, GetVal<BitcoinKernel.Chain.Chain>(chainRef).Height);
    }

    public Response ChainGetByHeight(string id, string? refName, BtckChainGetByHeightParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Chain?.Ref is not { } chainRef) return RefError(id);

        var blockIndex = GetVal<BitcoinKernel.Chain.Chain>(chainRef).GetBlockByHeight(p.BlockHeight);
        if (blockIndex == null) return Responses.EmptyError(id);

        _registry.Register(refName, new NonOwningRef<BlockIndex>(blockIndex));
        return Responses.Ref(id, refName);
    }

    public Response ChainContains(string id, BtckChainContainsParams p)
    {
        if (p.Chain?.Ref is not { } chainRef) return RefError(id);
        if (p.BlockTreeEntry?.Ref is not { } bteRef) return RefError(id);

        bool contains = GetVal<BitcoinKernel.Chain.Chain>(chainRef).Contains(GetVal<BlockIndex>(bteRef));
        return Responses.Ok(id, contains);
    }

    // ── Block ─────────────────────────────────────────────────────────────────

    public Response BlockCreate(string id, string? refName, BtckBlockCreateParams p)
    {
        if (refName == null) return RefError(id);

        try
        {
            var block = Block.FromBytes(Convert.FromHexString(p.RawBlock));
            _registry.Register(refName, block);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockGetHash(string id, string? refName, BtckBlockRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Block?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Block>(r).GetBlockHash());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockGetHeader(string id, string? refName, BtckBlockRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Block?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Block>(r).GetHeader());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockCopy(string id, string? refName, BtckBlockRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Block?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Block>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockCountTransactions(string id, BtckBlockRefParams p)
    {
        if (p.Block?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<Block>(r).TransactionCount);
    }

    public Response BlockGetTransactionAt(string id, string? refName, BtckBlockGetTransactionAtParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Block?.Ref is not { } r) return RefError(id);

        try
        {
            var tx = Get<Block>(r).GetTransaction(p.TransactionIndex);
            if (tx == null) return Responses.EmptyError(id);
            _registry.Register(refName, tx);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockToBytes(string id, BtckBlockRefParams p)
    {
        if (p.Block?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Hex(Get<Block>(r).ToBytes()));
    }

    public Response BlockDestroy(string id, BtckBlockRefParams p)
    {
        if (p.Block?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Block Hash ────────────────────────────────────────────────────────────

    public Response BlockHashCreate(string id, string? refName, BtckBlockHashCreateParams p)
    {
        if (refName == null) return RefError(id);

        try
        {
            var hash = BlockHash.FromBytes(Convert.FromHexString(p.BlockHashHex));
            _registry.Register(refName, hash);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockHashToBytes(string id, BtckBlockHashRefParams p)
    {
        if (p.BlockHash?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Hex(Get<BlockHash>(r).ToBytes()));
    }

    public Response BlockHashEquals(string id, BtckBlockHashEqualsParams p)
    {
        if (p.Hash1?.Ref is not { } r1) return RefError(id);
        if (p.Hash2?.Ref is not { } r2) return RefError(id);
        return Responses.Ok(id, Get<BlockHash>(r1).Equals(Get<BlockHash>(r2)));
    }

    public Response BlockHashCopy(string id, string? refName, BtckBlockHashRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.BlockHash?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<BlockHash>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockHashDestroy(string id, BtckBlockHashRefParams p)
    {
        if (p.BlockHash?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Block Header ──────────────────────────────────────────────────────────

    public Response BlockHeaderCreate(string id, string? refName, BtckBlockHeaderCreateParams p)
    {
        if (refName == null) return RefError(id);

        try
        {
            var header = BlockHeader.FromBytes(Convert.FromHexString(p.RawBlockHeader));
            _registry.Register(refName, header);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockHeaderToBytes(string id, BtckBlockHeaderRefParams p)
    {
        if (p.Header?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Hex(Get<BlockHeader>(r).ToBytes()));
    }

    public Response BlockHeaderGetHash(string id, string? refName, BtckBlockHeaderRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Header?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<BlockHeader>(r).GetBlockHash());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockHeaderGetPrevHash(string id, string? refName, BtckBlockHeaderRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Header?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<BlockHeader>(r).GetPrevBlockHash());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockHeaderGetVersion(string id, BtckBlockHeaderRefParams p)
    {
        if (p.Header?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<BlockHeader>(r).Version);
    }

    public Response BlockHeaderGetTimestamp(string id, BtckBlockHeaderRefParams p)
    {
        if (p.Header?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<BlockHeader>(r).Timestamp);
    }

    public Response BlockHeaderGetBits(string id, BtckBlockHeaderRefParams p)
    {
        if (p.Header?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<BlockHeader>(r).Bits);
    }

    public Response BlockHeaderGetNonce(string id, BtckBlockHeaderRefParams p)
    {
        if (p.Header?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<BlockHeader>(r).Nonce);
    }

    public Response BlockHeaderCopy(string id, string? refName, BtckBlockHeaderRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Header?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<BlockHeader>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response BlockHeaderDestroy(string id, BtckBlockHeaderRefParams p)
    {
        if (p.Header?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Block Tree Entry ──────────────────────────────────────────────────────

    public Response BlockTreeEntryGetBlockHash(string id, string? refName, BtckBlockTreeEntryGetBlockHashParams p)
    {
        if (refName == null) return RefError(id);
        if (p.BlockTreeEntry?.Ref is not { } bteRef) return RefError(id);

        try
        {
            _registry.Register(refName, GetVal<BlockIndex>(bteRef).GetBlockHash());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    // ── Script Pubkey ─────────────────────────────────────────────────────────

    public Response ScriptPubkeyCreate(string id, string? refName, BtckScriptPubkeyCreateParams p)
    {
        if (refName == null) return RefError(id);

        try
        {
            // Use FromBytes (not FromHex) so an empty script pubkey (empty hex) is accepted.
            var spk = ScriptPubKey.FromBytes(Convert.FromHexString(p.ScriptPubKeyHex));
            _registry.Register(refName, spk);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response ScriptPubkeyCopy(string id, string? refName, BtckScriptPubkeyRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.ScriptPubKey?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<ScriptPubKey>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response ScriptPubkeyToBytes(string id, BtckScriptPubkeyRefParams p)
    {
        if (p.ScriptPubKey?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Hex(Get<ScriptPubKey>(r).ToBytes()));
    }

    public Response ScriptPubkeyVerify(string id, BtckScriptPubkeyVerifyParams p)
    {
        if (p.ScriptPubKey?.Ref is not { } spkRef) return RefError(id);
        if (p.TxTo?.Ref is not { } txRef) return RefError(id);

        try
        {
            var scriptPubKey = Get<ScriptPubKey>(spkRef);
            var transaction = Get<Transaction>(txRef);

            PrecomputedTransactionData? precomputed = p.PrecomputedTxData?.Ref is { } precompRef
                ? Get<PrecomputedTransactionData>(precompRef)
                : null;

            var flags = ParseFlags(p.Flags);

            bool valid = ScriptVerifier.VerifyScript(
                scriptPubKey,
                p.Amount,
                transaction,
                precomputed,
                p.InputIndex,
                flags);

            return Responses.Ok(id, valid);
        }
        catch (ScriptVerificationException ex) when (ex.Status != ScriptVerifyStatus.OK)
        {
            return Responses.CodedError(id, "btck_ScriptVerifyStatus", MapScriptVerifyStatus(ex.Status));
        }
        catch (KeyNotFoundException)
        {
            return Responses.EmptyError(id);
        }
    }

    public Response ScriptPubkeyDestroy(string id, BtckScriptPubkeyDestroyParams p)
    {
        if (p.ScriptPubKey?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Transaction ───────────────────────────────────────────────────────────

    public Response TransactionCreate(string id, string? refName, BtckTransactionCreateParams p)
    {
        if (refName == null) return RefError(id);

        try
        {
            var tx = Transaction.FromHex(p.RawTransaction);
            _registry.Register(refName, tx);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionCopy(string id, string? refName, BtckTransactionRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Transaction?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Transaction>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionCountInputs(string id, BtckTransactionRefParams p)
    {
        if (p.Transaction?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<Transaction>(r).InputCount);
    }

    public Response TransactionCountOutputs(string id, BtckTransactionRefParams p)
    {
        if (p.Transaction?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<Transaction>(r).OutputCount);
    }

    public Response TransactionGetTxid(string id, string? refName, BtckTransactionRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Transaction?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Transaction>(r).GetTxid());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionToBytes(string id, BtckTransactionRefParams p)
    {
        if (p.Transaction?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Hex(Get<Transaction>(r).ToBytes()));
    }

    public Response TransactionGetInputAt(string id, string? refName, BtckTransactionGetInputAtParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Transaction?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Transaction>(r).GetInputAt(p.InputIndex));
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionGetOutputAt(string id, string? refName, BtckTransactionGetOutputAtParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Transaction?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Transaction>(r).GetOutputAt(p.OutputIndex));
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionDestroy(string id, BtckTransactionDestroyParams p)
    {
        if (p.Transaction?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Transaction Input ─────────────────────────────────────────────────────

    public Response TransactionInputGetOutPoint(string id, string? refName, BtckTransactionInputRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TransactionInput?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<TransactionInput>(r).GetOutPoint());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionInputCopy(string id, string? refName, BtckTransactionInputRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TransactionInput?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<TransactionInput>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionInputDestroy(string id, BtckTransactionInputRefParams p)
    {
        if (p.TransactionInput?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Transaction Out Point ─────────────────────────────────────────────────

    public Response TransactionOutPointGetIndex(string id, BtckTransactionOutPointRefParams p)
    {
        if (p.TransactionOutPoint?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<OutPoint>(r).Index);
    }

    public Response TransactionOutPointGetTxid(string id, string? refName, BtckTransactionOutPointRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TransactionOutPoint?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<OutPoint>(r).GetTxid());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionOutPointCopy(string id, string? refName, BtckTransactionOutPointRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TransactionOutPoint?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<OutPoint>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionOutPointDestroy(string id, BtckTransactionOutPointRefParams p)
    {
        if (p.TransactionOutPoint?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Txid ──────────────────────────────────────────────────────────────────

    public Response TxidToBytes(string id, BtckTxidRefParams p)
    {
        if (p.Txid?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Hex(Get<Txid>(r).ToBytes()));
    }

    public Response TxidEquals(string id, BtckTxidEqualsParams p)
    {
        if (p.Txid1?.Ref is not { } r1) return RefError(id);
        if (p.Txid2?.Ref is not { } r2) return RefError(id);
        return Responses.Ok(id, Get<Txid>(r1).Equals(Get<Txid>(r2)));
    }

    public Response TxidCopy(string id, string? refName, BtckTxidRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.Txid?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<Txid>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TxidDestroy(string id, BtckTxidRefParams p)
    {
        if (p.Txid?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Transaction Output ────────────────────────────────────────────────────

    public Response TransactionOutputCreate(string id, string? refName, BtckTransactionOutputCreateParams p)
    {
        if (refName == null) return Responses.Null(id);
        if (p.ScriptPubKey?.Ref is not { } spkRef) return Responses.Null(id);

        try
        {
            var spk = Get<ScriptPubKey>(spkRef);
            var txOut = new TxOut(spk, p.Amount);
            _registry.Register(refName, txOut);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.Null(id);
        }
    }

    public Response TransactionOutputCopy(string id, string? refName, BtckTransactionOutputRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TransactionOutput?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<TxOut>(r).Copy());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionOutputGetAmount(string id, BtckTransactionOutputRefParams p)
    {
        if (p.TransactionOutput?.Ref is not { } r) return RefError(id);
        return Responses.Ok(id, Get<TxOut>(r).Amount);
    }

    public Response TransactionOutputGetScriptPubkey(string id, string? refName, BtckTransactionOutputRefParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TransactionOutput?.Ref is not { } r) return RefError(id);

        try
        {
            _registry.Register(refName, Get<TxOut>(r).GetScriptPubkey());
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response TransactionOutputDestroy(string id, BtckTransactionOutputDestroyParams p)
    {
        if (p.TransactionOutput?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Precomputed Transaction Data ──────────────────────────────────────────

    public Response PrecomputedTransactionDataCreate(string id, string? refName, BtckPrecomputedTransactionDataCreateParams p)
    {
        if (refName == null) return RefError(id);
        if (p.TxTo?.Ref is not { } txRef) return RefError(id);

        try
        {
            var transaction = Get<Transaction>(txRef);

            List<TxOut>? spentOutputs = null;
            if (p.SpentOutputs is { Count: > 0 })
            {
                spentOutputs = p.SpentOutputs
                    .Select(r => Get<TxOut>(r.Ref))
                    .ToList();
            }

            var precomputed = new PrecomputedTransactionData(transaction, spentOutputs);
            _registry.Register(refName, precomputed);
            return Responses.Ref(id, refName);
        }
        catch
        {
            return Responses.EmptyError(id);
        }
    }

    public Response PrecomputedTransactionDataDestroy(string id, BtckPrecomputedTransactionDataDestroyParams p)
    {
        if (p.PrecomputedTxData?.Ref is { } r) _registry.Destroy(r);
        return Responses.Null(id);
    }

    // ── Parsing helpers ───────────────────────────────────────────────────────

    private static string Hex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

    private static ChainType ParseChainType(string s) => s switch
    {
        "btck_ChainType_MAINNET" => ChainType.MAINNET,
        "btck_ChainType_TESTNET" => ChainType.TESTNET,
        "btck_ChainType_TESTNET_4" => ChainType.TESTNET_4,
        "btck_ChainType_SIGNET" => ChainType.SIGNET,
        "btck_ChainType_REGTEST" => ChainType.REGTEST,
        _ => throw new ArgumentException($"Unknown chain type: {s}")
    };

    private static ScriptVerificationFlags ParseFlags(System.Text.Json.JsonElement? flags)
    {
        if (flags == null) return ScriptVerificationFlags.None;

        var el = flags.Value;
        if (el.ValueKind == System.Text.Json.JsonValueKind.Number)
            return (ScriptVerificationFlags)el.GetUInt32();

        if (el.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            var combined = ScriptVerificationFlags.None;
            foreach (var item in el.EnumerateArray())
                if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                    combined |= ParseFlagString(item.GetString() ?? string.Empty);
            return combined;
        }

        if (el.ValueKind == System.Text.Json.JsonValueKind.String)
            return ParseFlagString(el.GetString() ?? string.Empty);

        return ScriptVerificationFlags.None;
    }

    private static ScriptVerificationFlags ParseFlagString(string s)
    {
        const string prefix = "btck_ScriptVerificationFlags_";
        if (s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            s = s[prefix.Length..];

        return s.ToUpperInvariant() switch
        {
            "NONE" => ScriptVerificationFlags.None,
            "P2SH" => ScriptVerificationFlags.P2SH,
            "DERSIG" => ScriptVerificationFlags.DerSig,
            "NULLDUMMY" => ScriptVerificationFlags.NullDummy,
            "CHECKLOCKTIMEVERIFY" => ScriptVerificationFlags.CheckLockTimeVerify,
            "CHECKSEQUENCEVERIFY" => ScriptVerificationFlags.CheckSequenceVerify,
            "WITNESS" => ScriptVerificationFlags.Witness,
            "TAPROOT" => ScriptVerificationFlags.Taproot,
            _ => throw new ArgumentException($"Unknown script verification flag: {s}")
        };
    }

    private static string MapScriptVerifyStatus(ScriptVerifyStatus status) => status switch
    {
        ScriptVerifyStatus.ERROR_INVALID_FLAGS_COMBINATION => "ERROR_INVALID_FLAGS_COMBINATION",
        ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_REQUIRED => "ERROR_SPENT_OUTPUTS_REQUIRED",
        ScriptVerifyStatus.ERROR_TX_INPUT_INDEX => "ERROR_TX_INPUT_INDEX",
        ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_MISMATCH => "ERROR_SPENT_OUTPUTS_MISMATCH",
        ScriptVerifyStatus.ERROR_INVALID_FLAGS => "ERROR_INVALID_FLAGS",
        _ => "ERROR_UNKNOWN"
    };
}

/// <summary>
/// Wraps a <see cref="ChainstateManager"/> together with the temp directory it owns,
/// so both are cleaned up when the entry is removed from the registry.
/// </summary>
internal sealed class ChainstateManagerWithTempDir : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    internal ChainstateManagerWithTempDir(ChainstateManager manager, string tempDir)
    {
        Manager = manager;
        _tempDir = tempDir;
    }

    internal ChainstateManager Manager { get; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Manager.Dispose();
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best-effort */ }
    }
}
