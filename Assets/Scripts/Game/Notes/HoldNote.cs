﻿using System.Collections.Generic;
using UnityEngine;

public class HoldNote : Note
{
    private bool holding;
    public bool Holding
    {
        get => holding;
        set
        {
            if (holding != value)
            {
                holding = value;
                OnSetHolding(value);
            }
        }
    }
    public float HoldingStartTime  { get; protected set; }
    public float HeldDuration  { get; protected set; }
    public float HoldProgress { get; protected set; }
    public List<int> HoldingFingers { get; } = new List<int>(2);

    protected override NoteRenderer CreateRenderer()
    {
        return new HoldNoteRenderer(this);
    }

    protected virtual void OnSetHolding(bool holding)
    {
        if (holding)
        {
            HoldingStartTime = Game.Time;
        }
        else
        {
            if (Game.Time > Model.start_time && Game.State.IsPlaying)
            {
                Clear(IsAutoEnabled() ? NoteGrade.Perfect : CalculateGrade());
            }
        }
    }

    protected override void OnGameUpdate()
    {
        base.OnGameUpdate();
        if (Holding)
        {
            HeldDuration = Game.Time - HoldingStartTime;
            HoldProgress = (Game.Time - Model.start_time) / Model.Duration;

            // Already completed?
            if (Game.Time >= Model.end_time)
            {
                Holding = false;
            }
        }
        else
        {
            HoldProgress = 0;
        }
    }

    public override bool ShouldMiss()
    {
        return !Holding && base.ShouldMiss();
    }
    
    public override void OnTouch(Vector2 screenPos)
    {
        // Do nothing
    }

    public void SetHoldingBy(int finger, bool holding)
    {
        Holding = holding;
        if (holding)
        {
            HoldingFingers.Add(finger);
            Holding = true;
        }
        else
        {
            HoldingFingers.Remove(finger);
            if (HoldingFingers.Count == 0)
            {
                Holding = false;
            }
        }
    }

    public override NoteGrade CalculateGrade()
    {
        var grade = NoteGrade.Miss;
        var rankedGrade = NoteGrade.Miss;
        if (HeldDuration > Model.Duration - 0.05f) grade = NoteGrade.Perfect;
        else if (HeldDuration > Model.Duration * 0.7f) grade = NoteGrade.Great;
        else if (HeldDuration > Model.Duration * 0.5f) grade = NoteGrade.Good;
        else if (HeldDuration > Model.Duration * 0.3f) grade = NoteGrade.Bad;

        if (Game.State.IsRanked)
        {
            if (HoldingStartTime > Model.start_time)
            {
                var lateBy = HoldingStartTime - Model.start_time;
                if (lateBy < 0.200f) rankedGrade = NoteGrade.Bad;
                if (lateBy < 0.150f) rankedGrade = NoteGrade.Good;
                if (lateBy < 0.070f) rankedGrade = NoteGrade.Great;
                if (lateBy <= 0.040f) rankedGrade = NoteGrade.Perfect;
                if (rankedGrade == NoteGrade.Great) GreatGradeWeight = 1.0f - (lateBy - 0.040f) / (0.070f - 0.040f);
            }
            else
            {
                rankedGrade = grade;
                if (rankedGrade == NoteGrade.Great) GreatGradeWeight = 1.0f - (HeldDuration - Model.Duration * 0.70f) /
                                       (Model.Duration - 0.050f - Model.Duration * 0.70f);
            }
        }

        if (Game.State.IsRanked && rankedGrade > grade)
            return rankedGrade; // Return the "worse" ranking (Note miss > bad > good > great > perfect)
        return grade;
    }

    public override bool IsAutoEnabled()
    {
        return base.IsAutoEnabled() || Game.State.Mods.Contains(Mod.AutoHold);
    }
}