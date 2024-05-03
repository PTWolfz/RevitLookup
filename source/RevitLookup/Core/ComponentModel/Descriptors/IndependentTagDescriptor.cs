﻿// Copyright 2003-2024 by Autodesk, Inc.
// 
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
// 
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
// 
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.

using System.Reflection;
#if REVIT2025_OR_GREATER
using Autodesk.Revit.DB.Structure;
#endif
using RevitLookup.Core.Contracts;
using RevitLookup.Core.Objects;

namespace RevitLookup.Core.ComponentModel.Descriptors;

public sealed class IndependentTagDescriptor(IndependentTag tag) : ElementDescriptor(tag), IDescriptorResolver
{
    public new ResolveSet Resolve(Document context, string target, ParameterInfo[] parameters)
    {
        return target switch
        {
#if REVIT2025_OR_GREATER
            nameof(IndependentTag.TagText) when RebarBendingDetail.IsBendingDetail(tag) => ResolveSet.Append(new NotSupportedException("RebarBendingDetail not supported. Revit API critical Exception")),
#endif
            nameof(IndependentTag.CanLeaderEndConditionBeAssigned) => ResolveLeaderEndCondition(),
#if REVIT2022_OR_GREATER
            nameof(IndependentTag.GetLeaderElbow) => ResolveLeaderElbow(),
            nameof(IndependentTag.GetLeaderEnd) => ResolveLeaderEnd(),
            nameof(IndependentTag.HasLeaderElbow) => ResolveHasLeaderElbow(),
            nameof(IndependentTag.IsLeaderVisible) => ResolveIsLeaderVisible(),
#endif
            _ => null
        };
        
        ResolveSet ResolveLeaderEndCondition()
        {
            var conditions = Enum.GetValues(typeof(LeaderEndCondition));
            var resolveSummary = new ResolveSet(conditions.Length);
            
            foreach (LeaderEndCondition condition in conditions)
            {
                resolveSummary.AppendVariant(tag.CanLeaderEndConditionBeAssigned(condition), condition.ToString());
            }
            
            return resolveSummary;
        }
#if REVIT2022_OR_GREATER
        ResolveSet ResolveLeaderElbow()
        {
            var references = tag.GetTaggedReferences();
            var resolveSummary = new ResolveSet(references.Count);
            
            foreach (var reference in references)
            {
                if (tag.IsLeaderVisible(reference) && tag.HasLeaderElbow(reference))
                    resolveSummary.AppendVariant(tag.GetLeaderElbow(reference));
            }
            
            return resolveSummary;
        }
        
        ResolveSet ResolveLeaderEnd()
        {
            if (tag.LeaderEndCondition == LeaderEndCondition.Attached)
                return ResolveSet.Append("The tag has attached leader end condition, it has no LeaderEnd");
            var references = tag.GetTaggedReferences();
            var resolveSummary = new ResolveSet(references.Count);
            
            foreach (var reference in references)
            {
                if (tag.IsLeaderVisible(reference))
                    resolveSummary.AppendVariant(tag.GetLeaderEnd(reference));
            }
            
            return resolveSummary;
        }
        
        ResolveSet ResolveHasLeaderElbow()
        {
            var references = tag.GetTaggedReferences();
            var resolveSummary = new ResolveSet(references.Count);
            foreach (var reference in references)
            {
                if (tag.IsLeaderVisible(reference))
                    resolveSummary.AppendVariant(tag.HasLeaderElbow(reference));
            }
            
            return resolveSummary;
        }
        
        ResolveSet ResolveIsLeaderVisible()
        {
            var references = tag.GetTaggedReferences();
            var resolveSummary = new ResolveSet(references.Count);
            
            foreach (var reference in references)
            {
                resolveSummary.AppendVariant(tag.IsLeaderVisible(reference));
            }
            
            return resolveSummary;
        }
#endif
    }
}