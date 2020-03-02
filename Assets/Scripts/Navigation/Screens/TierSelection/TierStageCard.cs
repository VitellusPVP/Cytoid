using System;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using UniRx.Async;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TierStageCard : InteractableMonoBehavior
{
    public Image cover;
    public Text artist;
    public Text title;
    public Text titleLocalized;

    public DifficultyBall difficultyBall;
    public GradientMeshEffect overlayGradient;
    
    private Level level;
    private bool loadedCover;
    private CancellationTokenSource actionToken;
    private Vector2 pressPosition;

    public void SetModel(Level level, ColorGradient gradient)
    {
        this.level = level;
        
        artist.text = level.Meta.artist;
        title.text = level.Meta.title;
        titleLocalized.text = level.Meta.title_localized;
        titleLocalized.gameObject.SetActive(!string.IsNullOrEmpty(level.Meta.title_localized));

        difficultyBall.SetModel(Difficulty.Parse(level.Meta.charts[0].type), level.Meta.charts[0].difficulty);
        overlayGradient.SetGradient(gradient);

        LayoutFixer.Fix(transform);

        LoadCover();
    }

    private CancellationTokenSource coverToken = new CancellationTokenSource();

    public async void LoadCover()
    {
        loadedCover = false;
        cover.DOKill();
        cover.SetAlpha(0);
        if (coverToken != null
            && !coverToken.IsCancellationRequested)
        {
            coverToken.Cancel();
        }

        coverToken = new CancellationTokenSource();
        Sprite sprite = null;
        try
        {
            if (level.IsLocal)
            {
                var path = "file://" + level.Path + LevelManager.CoverThumbnailFilename;
                sprite = await Context.SpriteCache.CacheSpriteInMemory(path, "LocalLevelCoverThumbnail",
                    coverToken.Token);
            }
            else
            {
                var path = level.Meta.background.path.WithImageCdn().WithSizeParam(
                    Context.ThumbnailWidth, Context.ThumbnailHeight);
                sprite = await Context.SpriteCache.CacheSpriteInMemory(path, "RemoteLevelCoverThumbnail",
                    coverToken.Token, new []{ Context.ThumbnailWidth, Context.ThumbnailHeight }, useFileCache: true);
            }
        }
        catch
        {
            if (sprite != null)
            {
                // Should be impossible
                Destroy(sprite); 
            }
            return;
        }

        if (sprite != null)
        {
            lock (sprite)
            {
                if (sprite != null)
                {
                    if (cover == null) return;
                    cover.sprite = sprite;
                    cover.DOFade(0.5f, 0.2f);
                    cover.FitSpriteAspectRatio();
                    loadedCover = true;
                }
            }
        }
    }

}