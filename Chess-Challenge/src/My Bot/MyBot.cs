using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    // public uint NumberOfPositionSearched = 0; 
    public Move Think(Board board, Timer timer)
    {
        // NumberOfPositionSearched = 0;
        Console.WriteLine("started thinking ....");
        
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];

        int bestScore = -1000000;
        int alpha = -1000000;
        int beta = 1000000;
        Array.Sort(moves, (m1, m2) => MoveOrderingHeuristic(m1, board).CompareTo(MoveOrderingHeuristic(m2, board)));
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // NumberOfPositionSearched++;
            int score = -Negamax(board, 4, -beta, -alpha);
            board.UndoMove(move);
            // Console.WriteLine(move.StartSquare.Name + "" + move.TargetSquare.Name + " " + score);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            alpha = Math.Max(alpha, bestScore);
        }
        // Console.WriteLine("positions evalueated " + NumberOfPositionSearched);
        return bestMove;
        
        
    }

    int MoveOrderingHeuristic(Move move, Board board){
        int score = 0;
            var capturedPiece = board.GetPiece(move.TargetSquare);
            if (move.IsCapture)
            {
                int pieceValue = PieceValue(capturedPiece, board);
                int capturingPieceValue = PieceValue(board.GetPiece(move.StartSquare), board);
                score += 1000 * (pieceValue - capturingPieceValue); // Difference in piece values
            }
            // if (move.IsCheck)
            //     score += 100; // Check move, medium score
            return score;
    }

    int PieceValue(Piece piece, Board board){
        return piece.PieceType switch
                {
                    PieceType.Pawn => 100,
                    PieceType.Knight => 300 + BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(piece.Square)),
                    PieceType.Bishop => 300 + BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(piece.PieceType, piece.Square, board)),
                    PieceType.Rook => 500 + BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(piece.PieceType, piece.Square, board)),
                    PieceType.Queen => 900 + BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(piece.PieceType, piece.Square, board)),
                    PieceType.King => piece.IsWhite ? ((piece.Square == new Square(2) || piece.Square == new Square(6)) ? 5 : 0 ) : ((piece.Square == new Square(58) || piece.Square == new Square(62)) ? 5 : 0 ), 
                    _ => 0,
                };
    }

    int Negamax(Board board, int depth, int alpha, int beta) {
        if (depth <= 0 || board.IsInCheckmate() || board.IsDraw())
            return EvaluateBoard(board);

        if (depth > 1 && !board.IsInCheck()){
            board.MakeMove(new Move());
            int score = -Negamax(board, depth - 1, -beta, -beta + 1); // -AlphaBeta (0-beta, 1-beta, depth-R-1)

            if (score >= beta ) {
                // fail high on null move
                board.UndoMove(new Move());
                return beta;
            }
            board.UndoMove(new Move());
        }

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            // NumberOfPositionSearched++;
            int score = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (score >= beta)
                return beta; // Beta cutoff

            if (score > alpha)
                alpha = score;
        }

        return alpha;
    }

    int EvaluateBoard(Board board) {
        if (board.IsDraw())
            return 0; 
         if (board.IsInCheckmate())
                return int.MinValue / 2;

        int score = 0;
        foreach (PieceList list in board.GetAllPieceLists())
            foreach (var piece in list)
            {
                int pieceValue = PieceValue(piece, board); 

                score += piece.IsWhite ? pieceValue : -pieceValue;
            }

        return score * (board.IsWhiteToMove ? 1 : -1);
    }


}