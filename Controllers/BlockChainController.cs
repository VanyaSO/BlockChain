using BlockChain.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlockChain.Controllers;

public class BlockChainController : Controller
{
    private readonly BlockChainService _blockChainService;

    public BlockChainController(BlockChainService blockChainService)
    {
        _blockChainService = blockChainService;
    }

    public async Task<IActionResult> Index()
    {
        var validatedBlocks = await _blockChainService.GetValidatedBlocksAsync();
        ViewBag.IsChainValid = await _blockChainService.IsValidAsync();
        return View(validatedBlocks);
    }

    [HttpPost]
    public async Task<IActionResult> Add(string data)
    {
        await _blockChainService.AddBlockAsync(data);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int index)
    {
        var block = await _blockChainService.GetBlockByIndexAsync(index);
        if (block == null) return NotFound();
        return View(block);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int index, string data)
    {
        var result = await _blockChainService.EditBlockAsync(index, data);
        if (!result) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}