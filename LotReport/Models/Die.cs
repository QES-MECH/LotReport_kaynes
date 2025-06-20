﻿using System.Windows;
using System.Windows.Media;
using Framework.MVVM;

namespace LotReport.Models
{
    public class Die : PropertyChangedBase
    {
        private Point _coordinate;
        private BinCode _binCode;
        private string _diePath;
        private string _markPath;
        private Mark _markStatus;
        private Color _color;
        private bool _modified;
        private BinCode _binCode2D;
        private BinCode _binCode3D;
        private string _diePath3DLeft;
        private string _diePath3DRight;
        private string _diePath3DFront;
        private string _diePath3DBack;

        public Die()
        {
            BinCode = new BinCode();
            BinCode2D = new BinCode();
            BinCode3D = new BinCode();
        }

        public enum Mark
        {
            NA,
            Pass,
            Fail
        }

        public Point Coordinate { get => _coordinate; set => SetProperty(ref _coordinate, value); }

        public BinCode BinCode { get => _binCode; set => SetProperty(ref _binCode, value); }

        public string DiePath { get => _diePath; set => SetProperty(ref _diePath, value); }

        public string MarkPath { get => _markPath; set => SetProperty(ref _markPath, value); }

        public Mark MarkStatus { get => _markStatus; set => SetProperty(ref _markStatus, value); }

        public Color Color { get => _color; set => SetProperty(ref _color, value); }

        public bool Modified { get => _modified; set => SetProperty(ref _modified, value); }

        public bool Reviewed { get; set; }

        public BinCode BinCode2D { get => _binCode2D; set => SetProperty(ref _binCode2D, value); }

        public BinCode BinCode3D { get => _binCode3D; set => SetProperty(ref _binCode3D, value); }

        public string DiePath3DLeft { get => _diePath3DLeft; set => SetProperty(ref _diePath3DLeft, value); }

        public string DiePath3DRight { get => _diePath3DRight; set => SetProperty(ref _diePath3DRight, value); }

        public string DiePath3DFront { get => _diePath3DFront; set => SetProperty(ref _diePath3DFront, value); }

        public string DiePath3DBack { get => _diePath3DBack; set => SetProperty(ref _diePath3DBack, value); }
    }
}
