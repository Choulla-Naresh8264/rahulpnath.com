---
author: rahulpnath
comments: true
date: 2012-01-27 21:32:54+00:00
layout: post
slug: wpf-expander-trigger-on-isexpanded-to-change-the-header
title: WPF Expander trigger on IsExpanded to change the header
wordpress_id: 297
categories:
- WPF
tags:
- WPF
---

Just a quick tip on how you could change the Expander header content when Expander is in expanded state.I have also modified the expander HeaderTemplate so that the text gets center aligned.

```xml
 <Expander  Height="100" HorizontalAlignment="Left" Margin="129,192,0,0"
           Name="expander1" VerticalAlignment="Top" Width="167">
            <Expander.HeaderTemplate>
                <DataTemplate>
                    <Label Name="headerlabel"
                        Content="{Binding RelativeSource={RelativeSource
                        Mode=FindAncestor,
                        AncestorType={x:Type Expander}},
                        Path=Header}"
                        HorizontalContentAlignment="Center"
                        Width="{Binding
                        RelativeSource={RelativeSource
                        Mode=FindAncestor,
                        AncestorType={x:Type Expander}},
                        Path=ActualWidth}" />
                </DataTemplate>
            </Expander.HeaderTemplate>
            <Expander.Style>
                <Style  TargetType="Expander">
                    <Setter Property="Header" Value="Show"/>
                    <Style.Triggers>
                        <Trigger Property="IsExpanded" Value="True">
                            <Setter Property="Header" Value="Hide"/>
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </Expander.Style>
            <Grid >
                <Label>Expander Content</Label>
            </Grid>
        </Expander>
```


We could also end up doing this using a converter or explicitly handling for the Expanded/Collapsed events.
